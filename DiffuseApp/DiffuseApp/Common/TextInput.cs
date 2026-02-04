using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Diffuse.Common
{
    public class TextInput
    {
        private readonly string _textValue;
        private readonly string _sourceFile;

        public TextInput(string textInput)
        {
            _textValue = textInput;
        }

        public TextInput(string filename, Encoding encoding)
            : this(File.ReadAllText(filename, encoding))
        {
            _sourceFile = filename;
        }

        public string Text => _textValue;
        public string SourceFile => _sourceFile;
        public int Length => _textValue?.Length ?? 0;


        public int Beam { get; set; }
        public float Score { get; set; }
        public float PenaltyScore { get; set; }

        public void Save(string filename)
        {
            File.WriteAllText(filename, _textValue);
        }

        public async Task SaveAsync(string filename)
        {
            await File.WriteAllTextAsync(filename, _textValue);
        }

        public static async Task<TextInput> CreateAsync(string filename, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return new TextInput(await File.ReadAllTextAsync(filename, encoding, cancellationToken));
        }
    }

}
