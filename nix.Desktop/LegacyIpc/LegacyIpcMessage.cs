// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using nix.Framework.Platform;

using Newtonsoft.Json.Linq;

namespace nix.Desktop.LegacyIpc
{
    /// <summary>
    /// um <see cref="IpcMessage"/> que pode ser utilizado para comunicar clientes legacy
    /// <para>
    /// para desserializar tipos em qualquer extremidade, os tipos devem ser serializados como seus <see cref="System.Type.AssemblyQualifiedName"/>,
    /// no entanto, isso não pode ser feito, pois nix!stable e nix!lazer vivem em duas montagens diferentes.
    /// <br/>
    /// para contornar isso, existe esta classe que serializa um payload (<see cref="LegacyIpcMessage.Data"/>) como um tipo <see cref="System.Object"/>,
    /// que pode ser desserializado em qualquer extremidade porque faz parte da biblioteca principal (mscorlib / System.Private.CorLib).
    ///
    /// o payload contém os dados a serem enviados pelo canal ipc.
    /// <br/>
    /// em qualquer uma das extremidades, o json.net desserializa a carga em um <see cref="JObject"/> que é convertido manualmente de volta no tipo esperado <see cref="LegacyIpcMessage.Data"/>,
    /// que então contém outro <see cref="JObject"/> representando os dados enviados pelo canal ipc, cujo tipo também pode ser correspondido preguiçosamente por meio de um <see cref="LegacyIpcMessage.Data.MessageType"/>.
    /// </para>
    /// </summary>
    ///
    /// <remarks>
    /// sincroniza quaisquer alterações com nix-stable.
    /// </remarks>

    public class LegacyIpcMessage : IpcMessage
    {
        public LegacyIpcMessage()
        {
            // tipos/assemblies não são intercompatíveis, então sempre serializar/desserializar em objetos.
            base.Type = typeof(object).FullName;
        }

        public new string Type => base.Type; // esconder setter.

        public new object Value
        {
            get => base.Value;
            set => base.Value = new Data(value.GetType().Name, value);
        }

        public class Data
        {
            public string MessageType { get; }
            public object MessageData { get; }

            public Data(string messageType, object messageData)
            {
                MessageType = messageType;
                MessageData = messageData;
            }
        }
    }
}