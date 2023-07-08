// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using nix.Game.Database;

namespace nix.Game
{
    public partial class NixGameBase
    {
        private readonly List<ICanAcceptFiles> fileImporters = new List<ICanAcceptFiles>();

        /// <summary>
        /// registrar handler global para importações de arquivos.
        /// os registrados mais recentemente receberão precedência.
        /// </summary>
        ///
        /// <param name="handler"> o handler para ser registrado </param>
        public void RegisterImportHandler(ICanAcceptFiles handler) => fileImporters.Insert(0, handler);

        /// <summary>
        /// não registrar handler global para importações de arquivos.
        /// </summary>
        ///
        /// <param name="handler"> o handler registrado anteriormente </param>
        public void UnregisterImportHandler(ICanAcceptFiles handler) => fileImporters.Remove(handler);

        public async Task Import(params string[] paths)
        {
            if (paths.Length == 0)
                return;

            var filesPerExtension = paths.GroupBy(p => Path.GetExtension(p).ToLowerInvariant());

            foreach (var groups in filesPerExtension)
            {
                foreach (var importer in fileImporters)
                {
                    if (importer.HandledExtensions.Contains(groups.Key))
                        await importer.Import(groups.ToArray()).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task Import(ImportTask[] tasks, ImportParameters parameters = default)
        {
            var tasksPerExtension = tasks.GroupBy(t => Path.GetExtension(t.Path).ToLowerInvariant());

            await Task.WhenAll(tasksPerExtension.Select(taskGroup =>
            {
                var importer = fileImporters.FirstOrDefault(i => i.HandledExtensions.Contains(taskGroup.Key));
                
                return importer?.Import(taskGroup.ToArray(), parameters) ?? Task.CompletedTask;
            })).ConfigureAwait(false);
        }

        public IEnumerable<string> HandledExtensions => fileImporters.SelectMany(i => i.HandledExtensions);
    }
}