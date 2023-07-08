// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa

using System.IO;
using System.Threading.Tasks;

using Android.Content;
using Android.Net;
using Android.Provider;

using nix.Game.Database;

namespace nix.Android
{
    public class AndroidImportTask : ImportTask
    {
        private readonly ContentResolver contentResolver;

        private readonly Uri uri;

        private AndroidImportTask(Stream stream, string filename, ContentResolver contentResolver, Uri uri)
            : base(stream, filename)
        {
            this.contentResolver = contentResolver;
            this.uri = uri;
        }

        public override void DeleteFile()
        {
            contentResolver.Delete(uri, null, null);
        }

        public static async Task<AndroidImportTask?> Create(ContentResolver contentResolver, Uri uri)
        {
            // existem sobrecargas de maior desempenho deste método, mas este é o mais compatível com versões anteriores
            // (as dates voltam para a API 1)

            var cursor = contentResolver.Query(uri, null, null, null, null);

            if (cursor == null)
                return null;

            if (!cursor.MoveToFirst())
                return null;

            int filenameColumn = cursor.GetColumnIndex(IOpenableColumns.DisplayName);
            string filename = cursor.GetString(filenameColumn) ?? uri.Path ?? string.Empty;

            // SharpCompress requer fluxos de arquivo para serem pesquisáveis, que o fluxo aberto por
            // OpenInputStream() parece não ser necessariamente
            //
            // copie para um fluxo de memória de acesso arbitrário para poder prosseguir com a importação
            var copy = new MemoryStream();

            using (var stream = contentResolver.OpenInputStream(uri))
            {
                if (stream == null)
                    return null;

                await stream.CopyToAsync(copy).ConfigureAwait(false);
            }

            return new AndroidImportTask(copy, filename, contentResolver, uri);
        }
    }
}