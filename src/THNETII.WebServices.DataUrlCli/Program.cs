using System;
using System.Buffers;
using System.Buffers.Text;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace THNETII.WebServices.DataUrlCli
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var mimeOption = new Option("--mime")
            {
                Name = "mime",
                Description = "MIME type indicating the type of data",
                Argument = new Argument<MediaTypeHeaderValue>
                {
                    Name = "MIME",
                    Description = "RFC 2616 compliant Media-Type header value, like text/plain",
                    Arity = ArgumentArity.ZeroOrOne
                }
            };
            mimeOption.AddAlias("-m");
            mimeOption.Argument.AddValidator(symbol => symbol.Tokens.Select(t => t.Value)
                .Select(m =>
                {
                    try
                    {
                        _ = MediaTypeHeaderValue.Parse(m);
                        return null;
                    }
                    catch (FormatException)
                    {
                        return $"Invalid MIME format: '{m}'";
                    }
                })
                .FirstOrDefault(msg => msg is string)
                );

            var charsetOption = new Option("--charset")
            {
                Name = "charset",
                Description = "Add charset to mime-type",
                Argument = new Argument
                {
                    Name = "ENCODING",
                    Description = "Text encoding web name",
                    Arity = ArgumentArity.ZeroOrOne
                }
            };
            charsetOption.AddAlias("-c");
            charsetOption.Argument.AddSuggestions(Encoding
                .GetEncodings()
                .Select(i => i.Name)
                .ToList());

            var fileArgument = new Argument<FileInfo>
            {
                Name = "FILE",
                Description = "Path to file. Omit or use '-' for STDIN",
                Arity = ArgumentArity.ZeroOrOne
            };
            fileArgument.AddValidator(symbol => symbol.Tokens
                .Select(t => t.Value)
                .Where(p => p != "-" && !File.Exists(p))
                .Select(ValidationMessages.Instance.FileDoesNotExist)
                .FirstOrDefault()
                );

            var rootCommand = new RootCommand(typeof(Program).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description)
            {
                Handler = CommandHandler.Create<ParseResult, CancellationToken>(RunAsync)
            };
            rootCommand.AddArgument(fileArgument);

            var parser = new CommandLineBuilder(rootCommand)
                .AddOption(mimeOption)
                .AddOption(charsetOption)
                .UseDefaults()
                .Build();

            return parser.InvokeAsync(args);

            async Task RunAsync(ParseResult parseResult, CancellationToken cancelToken)
            {
                var mimeValue = parseResult.FindResultFor(mimeOption)?
                    .GetValueOrDefault<MediaTypeHeaderValue>();
                var charsetValue = parseResult.FindResultFor(charsetOption)?
                    .GetValueOrDefault<string>();

                if (charsetValue is string)
                {
                    if (mimeValue is null)
                        mimeValue = new MediaTypeHeaderValue("text/plain");
                    if (string.IsNullOrEmpty(mimeValue.CharSet))
                        mimeValue.CharSet = charsetValue;
                }

                var mimeString = mimeValue?.ToString();

                var fileResult = parseResult.FindResultFor(fileArgument);
                Stream fileStream;
                if (fileResult is null || fileResult.Tokens.Single().Value == "-")
                    fileStream = Console.OpenStandardInput();
                else
                {
                    var fileInfo = fileResult.GetValueOrDefault<FileInfo>();
                    fileStream = fileInfo.OpenRead();
                }

                using (fileStream)
                {
                    Console.Write("data:");
                    Console.Write(mimeString);
                    Console.Write(";base64,");

                    ArrayPool<char> charPool = ArrayPool<char>.Shared;
                    ArrayPool<byte> bytePool = ArrayPool<byte>.Shared;

                    var byteBufferSize = Base64.GetMaxDecodedFromUtf8Length(4096 / 2 - 1);
                    var byteRentedArray = bytePool.Rent(byteBufferSize);
                    try
                    {
                        char[] charBuffer;
                        Memory<byte> bytePreviousRemainder = Memory<byte>.Empty;
                        Memory<byte> byteCurrentRemainder = byteRentedArray;

                        for (int bytesRead = await fileStream.ReadAsync(byteCurrentRemainder, cancelToken).ConfigureAwait(false);
                            bytesRead != 0;
                            bytesRead = await fileStream.ReadAsync(byteCurrentRemainder, cancelToken).ConfigureAwait(false))
                        {
                            var byteAvailable = new Memory<byte>(byteRentedArray, 0, bytePreviousRemainder.Length + bytesRead);
                            var byteCount = (byteAvailable.Length / 3) * 3;
                            var byteBase64Bounded = byteAvailable.Slice(0, byteCount);
                            charBuffer = charPool.Rent(Base64.GetMaxEncodedToUtf8Length(byteBase64Bounded.Length));
                            try
                            {
                                bool base64Success = Convert.TryToBase64Chars(
                                    byteBase64Bounded.Span, charBuffer,
                                    out int charWritten,
                                    Base64FormattingOptions.None);
                                if (!base64Success)
                                    throw new InvalidOperationException($"{nameof(Convert)}.{nameof(Convert.TryToBase64Chars)} returned {base64Success}");

                                await Console.Out.WriteAsync(new ReadOnlyMemory<char>(charBuffer, 0, charWritten), cancelToken)
                                    .ConfigureAwait(false);
                            }
                            finally { charPool.Return(charBuffer); }

                            byteCurrentRemainder = byteAvailable.Slice(byteCount);
                            byteCurrentRemainder.CopyTo(byteRentedArray);
                            bytePreviousRemainder = new Memory<byte>(byteRentedArray, 0, byteCurrentRemainder.Length);
                            byteCurrentRemainder = new Memory<byte>(byteRentedArray).Slice(bytePreviousRemainder.Length);
                        }

                        charBuffer = charPool.Rent(Base64.GetMaxEncodedToUtf8Length(bytePreviousRemainder.Length));
                        try
                        {
                            bool base64Success = Convert.TryToBase64Chars(
                                    bytePreviousRemainder.Span, charBuffer,
                                    out int charWritten,
                                    Base64FormattingOptions.None);
                            if (!base64Success)
                                throw new InvalidOperationException($"{nameof(Convert)}.{nameof(Convert.TryToBase64Chars)} returned {base64Success}");

                            await Console.Out.WriteAsync(new ReadOnlyMemory<char>(charBuffer, 0, charWritten), cancelToken)
                                    .ConfigureAwait(false);
                        }
                        finally { charPool.Return(charBuffer); }
                    }
                    finally
                    {
                        bytePool.Return(byteRentedArray, clearArray: false);
                    }

                    await Console.Out.FlushAsync().ConfigureAwait(false);
                }

                Console.WriteLine();
            }
        }

        private static bool TryConvertEncoding(SymbolResult symbol, out Encoding encoding)
        {
            try
            {
                encoding = Encoding.GetEncoding(symbol.Tokens.First(t => !string.IsNullOrEmpty(t?.Value)).Value);
                return true;
            }
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            encoding = null;
            return false;
        }
    }
}
