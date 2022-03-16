using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Bigotes.Util
{
    public static class Error
    {
        private static string aviso = "`[ERROR]` ```Mensaje-de-error: {0} ```";

        public static async Task MostrarError(DiscordChannel channel, string errorMsg)
        {
            GuardarError(errorMsg);

            errorMsg = errorMsg.Replace(' ', '-');

            await channel.SendMessageAsync(aviso.Replace("{0}", errorMsg)).ConfigureAwait(false);
        }

        private static void GuardarError(string errorMsg)
        {
            //TODO
        }
    }
}
