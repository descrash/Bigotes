using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("saluda")]
        public async Task Greetings(CommandContext comCtx)
        {
            comCtx.Channel.SendMessageAsync("*[PROCESANDO SALUDO]* Hola. *[CORDIALIDAD AL 40%...]* ¿Qué tal?").ConfigureAwait(false);
        }
    }
}
