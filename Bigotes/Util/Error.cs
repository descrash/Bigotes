using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Util
{
    public static class Error
    {
        private static string aviso = "`[ERROR]` ```Mensaje-de-error: {0} ```";

        public static async Task MostrarError(CommandContext ctx, string errorMsg)
        {
            GuardarError(errorMsg);

            errorMsg = errorMsg.Replace(' ', '-');

            await ctx.Channel.SendMessageAsync(aviso.Replace("{0}", errorMsg)).ConfigureAwait(false);
        }

        private static void GuardarError(string errorMsg)
        {
            //TODO
        }
    }
}
