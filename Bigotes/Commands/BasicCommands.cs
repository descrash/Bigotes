using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando de saludo
        /// </summary>
        /// <param name="comCtx"></param>
        /// <returns></returns>
        [Command("saluda")]
        public async Task Greetings(CommandContext ctx)
        {
            ctx.Channel.SendMessageAsync("`[PROCESANDO SALUDO]` ```Hola.``` `[CORDIALIDAD AL 40%...]` ```¿Qué tal?```").ConfigureAwait(false);
        }
    }
}
