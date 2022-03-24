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

            try
            {
                Dices dices = new Dices();

                output = dices.Roll(tirada.Trim());

                if (output.Last() == "[1]" && output.Count == 1)
                {
                    msg = "`[CARGANDO RESULTADO: " + output.First() + ", DESASTROSO]` ```Resultado: [PIFIA]``` `[ERROR EN MENSAJE DE ÁNIMO]` ```Aconsejable-realizar-la-creación-de-una-ficha-nueva.``` `[CARGANDO GRABACIÓN DE VOZ DE MURETHOR]` ```[S'a matao Paco]```";
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
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, $"{ex.Message} Sugerencia: Utilice-el-formato-00d00-con-sumas-y-restas-adicionales." +
                    "Rango-del-entero-registrado-por-unidad-de-procesamiento-de-Bigotes: -2.147.483.648 a 2.147.483.648.");
            }
        }

        /// <summary>
        /// Función para devolver una consecuencia aleatoria de haber sufrido una pifia
        /// </summary>
        /// <returns></returns>
        public string ConsecuenciaPifia()
        {
            string consecuencia = String.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return consecuencia;
        }
    }
}
