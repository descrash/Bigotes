using Bigotes.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class DiceCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando de tiradas de dados
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [Command("roll")]
        [Description("Efectúa una tirada de dados con el formato 00d00 +/- 00.")]
        //[RequireRoles(RoleCheckMode.Any, "@everyone")]
        public async Task Roll(CommandContext ctx, [Description("Tirada de dados.")][RemainingText]string tirada)
        {
            List<string> output = new List<string>();
            string values = String.Empty,totalValue = String.Empty, msg;
            Dices dices = new Dices();

            output = dices.Roll(tirada.Trim());

            if (output == null || output.Count == 0)
            {
                msg = "`[CARGANDO MENSAJE DE ERROR]` ```Error-en-ejecución-de-comando.``` `[CARGANDO SUGERENCIA]` ```Por-favor,-utilice-el-formato-00d00-sumando-de-cero-a-infinito-añadidos).```";
            }
            else if (output.Last() == "Resultado: [1]")
            {
                msg = "`[CARGANDO RESULTADO: " + output.First() + ", DESASTROSO]` ```Resultado: [PIFIA]``` `[ERROR EN MENSAJE DE ÁNIMO]` ```Aconsejable-realizar-la-creación-de-una-ficha-nueva.```";
            }
            else
            {
                foreach (string str in output)
                {
                    if (!str.Contains("Resultado"))
                        values += " " + str;
                    else
                        totalValue = str;
                }

                msg = "`[CARGANDO RESULTADOS:" + values + " ]`\n`[CALCULANDO BONIFICADORES Y PENALIZADORES]` ```" + totalValue + ".```";
            }

            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
        }
    }
}
