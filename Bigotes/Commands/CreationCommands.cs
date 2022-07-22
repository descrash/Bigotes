using Bigotes.Clases;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bigotes.Util;
using DSharpPlus;
using System.Diagnostics;

namespace Bigotes.Commands
{
    public class CreationCommands : BaseCommandModule
    {
        /// <summary>
        /// Método para la creación de distintas utilidades
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="peticion"></param>
        /// <returns></returns>
        [Command("crear")]
        [Description("Creación de distintas utilidades: CREAR ENCUESTA, CREAR FICHA O CREAR EVENTO.")]
        public async Task Create(CommandContext ctx, [Description("Nombre de la utilidad: encuesta, ficha o evento.")][RemainingText]string peticion)
        {
            try
            {
                var interactivity = ctx.Client.GetInteractivity();

                switch (peticion.Trim().ToUpper())
                {
                    case "ENCUESTA":
                        await ctx.Channel.SendMessageAsync("`[ABRIENDO CANAL PRIVADO PARA CREACIÓN DE ENCUESTA]`").ConfigureAwait(false);
                        await Poll(ctx);
                        break;

                    case "FICHA":
                        throw new Exception("La creción de fichas está en mantenimiento a causa de futuros cambios en el sistema. Perdón por las molestias.");

                        await ctx.Channel.SendMessageAsync("`[ABRIENDO CANAL PRIVADO PARA CREACIÓN DE FICHA. DESCARGANDO TUTORIAL...]`");
                        await Task.Delay(2000);
                        await Ficha(ctx);
                        break;

                    case "EVENTO":
                        await ctx.Channel.SendMessageAsync("`[ABRIENDO FORMULARIO DE EVENTO]` ```Aviso: Al-ser-necesarias-menciones-de-usuarios, deberá-hacerse-en-público.").ConfigureAwait(false);
                        await Evento(ctx);
                        break;

                    default:
                        throw new Exception("Opción de creación no encontrada.");
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Método para la realización de encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="titulo"></param>
        /// <returns></returns>
        public async Task Poll(CommandContext ctx)
        {
            try
            {
                #region Atributos
                TimeSpan duration = new TimeSpan();
                DiscordMember author = ctx.Member;
                DiscordChannel canalEncuesta = ctx.Channel;
                string titulo = String.Empty;
                string description = String.Empty;
                string numOptions = "0";
                string urlEncuesta = String.Empty;
                List<DiscordEmoji> emojiOptions = new List<DiscordEmoji>();
                var interactivity = ctx.Client.GetInteractivity();
                DiscordMessage interMSG;
                #endregion

                #region Título de la encuesta
                await author.SendMessageAsync("`[ABRIENDO FORMULARIO DE ENCUESTA]` ```Inserte-título-de-la-encuesta:```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                titulo = interMSG.Content;
                #endregion

                #region Recogida y gestión de la duración de la encuesta
                await author.SendMessageAsync("`[TÍTULO REGISTRADO]` ```Orden-recibida.-Preciso-concretar-duración-en-minutos(m),-horas(h)-o-días(d):```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                string durationSTR = interMSG.Content.Trim(), unidadRespuesta;
                int durationINT = 0;

                while (!Regex.IsMatch(durationSTR, @"\d[mhd]"))
                {
                    await author.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-00m,-00h-o-00d.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                    durationSTR = interMSG.Content.Trim();
                }

                if (durationSTR.Contains("m"))
                {
                    durationINT = Math.Abs(Int32.Parse(durationSTR.Split('m')[0]));
                    duration = new TimeSpan(0, durationINT, 0);
                    unidadRespuesta = durationINT == 1 ? "minuto" : "minutos";
                }
                else if (durationSTR.Contains("h"))
                {
                    durationINT = Math.Abs(Int32.Parse(durationSTR.Split('h')[0]));
                    duration = new TimeSpan(durationINT, 0, 0);
                    unidadRespuesta = durationINT == 1 ? "hora" : "horas";
                }
                else
                {
                    durationINT = Math.Abs(Int32.Parse(durationSTR.Split('d')[0]));
                    duration = new TimeSpan(durationINT, 0, 0, 0);
                    unidadRespuesta = durationINT == 1 ? "día" : "días";
                }
                #endregion

                #region Recogida y gestión del canal en el que realizar la encuesta
                await author.SendMessageAsync("`[PROCESANDO DURACIÓN]` ```Programada-duración-de-" + durationINT + "-" + unidadRespuesta + ".``` ```Detallar-nombre-de-canal-de-encuesta.-Libertad-de-tomar-canal-actual-en-caso-de-no-existir-petición.```").ConfigureAwait(false);

                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                string channelName = interMSG.Content;
                canalEncuesta = Consultas.GetChannel(ctx, channelName);
                #endregion

                #region Recogida y gestión de la descripción
                await author.SendMessageAsync("`[BUSCANDO CANAL]` ```Canal-de encuesta-seleccionado:-" + canalEncuesta.Name + ".``` ```Se-precisa-texto-descriptivo:```").ConfigureAwait(false);

                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                description = interMSG.Content;
                #endregion

                #region Registro de número de opciones
                await author.SendMessageAsync("`[RECOGIENDO DESCRIPCIÓN]` ```¿Número-de-opciones?```");

                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                numOptions = interMSG.Content;
                while (!Regex.IsMatch(numOptions, @"\d"))
                {
                    await author.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-de-numero-entero-positivo.```").ConfigureAwait(false);
                    numOptions = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result.Content;
                }
                #endregion

                #region Obtención de emojis para las opciones (reacciones)
                var emojiMSG = await author.SendMessageAsync("`[NÚMERO AJUSTADO]` ```Necesarias-reacciones-a-este-mensaje-con-los-emojis-utilizados-en-cada-opción.```");

                while (emojiOptions.Count < Int32.Parse(numOptions))
                {
                    var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == emojiMSG && x.User == author).ConfigureAwait(false);

                    emojiOptions.Add(reactionResult.Result.Emoji);

                    await author.SendMessageAsync("```Procesada-opción-" + emojiOptions.Count + ":" + reactionResult.Result.Emoji.Name + "```").ConfigureAwait(false);
                }
                #endregion

                #region Creación de encuesta
                await author.SendMessageAsync("`[OPCIONES RECOGIDAS]` ```Se-han-recogido-las-opciones.-Procesando-encuesta...```");

                await Task.Delay(1500);

                var pollEmbed = new DiscordEmbedBuilder
                {
                    Title = titulo,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = author.DisplayName,
                        IconUrl = author.AvatarUrl
                    },
                    Description = description,
                    Color = DiscordColor.Blue
                };

                var pollMessage = await canalEncuesta.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

                foreach (var option in emojiOptions)
                {
                    await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
                }
                #endregion

                #region Finalización de encuesta
                var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
                var distinctResult = result.Distinct();

                List<string> results = new List<string>();

                foreach (var _emoji in emojiOptions)
                {
                    int votos = 0;
                    if (distinctResult.Where(x => x.Emoji == _emoji).Count() > 0)
                    {
                        votos = distinctResult.Where(x => x.Emoji == _emoji).FirstOrDefault().Total;
                    }
                    results.Add($"{_emoji}: {votos}");
                }

                var msgFinal = "`ENCUESTA " + titulo + " FINALIZADA` ```Resultados:```";

                await pollMessage.RespondAsync(msgFinal + string.Join("\n", results));
                #endregion
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Método para la realización de una ficha de personaje
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public async Task Ficha(CommandContext ctx)
        {
            try
            {
                #region Atributos
                Ficha nuevaFicha = new Ficha();
                DiscordEmoji emoji = null;
                DiscordMember author = ctx.Member;
                var interactivity = ctx.Client.GetInteractivity();
                DiscordMessage interMSG;
                DiscordColor color = DiscordColor.Yellow;
                int currentButtonsBuilder = 0; //Para controlar en qué mensaje de botones se encuentra
                #endregion

                #region DATOS BÁSICOS
                #region Nombre y apellidos
                await author.SendMessageAsync("```Ficha-en-blanco-preparada. Comenzando-por-DATOS-BÁSICOS. 1: Nombre.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == ctx.Member && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.nombre_completo = interMSG.Content;

                await author.SendMessageAsync("`[NOMBRE GUARDADO: " + nuevaFicha.nombre_completo + "]` ```2: Apellidos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == ctx.Member && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.apellidos = interMSG.Content;
                #endregion

                #region Género
                var genderBtnBuilder = Utiles.crearMensajeDeBotones(
                        ctx,
                        "`[APELLIDOS GUARDADOS: " + nuevaFicha.apellidos + "]` ```3: Elegir-género-entre-masculino, femenino-o-no-binario:```",
                        new string[] { "MASCULINO", "FEMENINO", "NOBINARIO" },
                        new string[] { "", "", "" },
                        null,
                        new string[] { ":male_sign:", ":female_sign:", ":transgender_symbol:" }
                    );

                await author.SendMessageAsync(genderBtnBuilder).ConfigureAwait(false);

                //var result = await MessageExtensions.WaitForButtonAsync(genderMSG);
                ctx.Client.ComponentInteractionCreated += async (s, e) =>
                {
                    ///Probamos con una variable para indicar en CUÁL de ambas opciones está. Podrían estar cruzándose los hilos.
                    if (currentButtonsBuilder == 0)
                    {

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("`[GÉNERO GUARDADO: " + e.Id + "]` ```4: Año-de-nacimiento.```"));

                        //Se usan los IDs directamente para ligarlo al enumerador
                        nuevaFicha.genero = (Util.Utiles.Genero)Enum.Parse(typeof(Util.Utiles.Genero), e.Id.ToString().ToUpper());

                    }
                };
                #endregion

                #region Edad
                //await author.SendMessageAsync("```4: Año-de-nacimiento.```").ConfigureAwait(false);
                int anio;
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                while (!Int32.TryParse(interMSG.Content, out anio))
                {
                    await author.SendMessageAsync("`[ERROR]` ```Insertar-número-válido-para-el-año-de-nacimiento.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                }
                nuevaFicha.bird_year = anio;
                #endregion

                #region Raza
                currentButtonsBuilder = 1;

                var humanoEmoji = DiscordEmoji.FromName(ctx.Client, ":humano:");
                var charrEmoji = DiscordEmoji.FromName(ctx.Client, ":charr:");
                var nornEmoji = DiscordEmoji.FromName(ctx.Client, ":norn:");
                var asuraEmoji = DiscordEmoji.FromName(ctx.Client, ":asura:");
                var sylvariEmoji = DiscordEmoji.FromName(ctx.Client, ":sylvari:");
                ///Dado que el mensaje sólo acepta CINCO BOTONES, descartaremos "otros" por el
                ///momento
                //var mundoEmoji = DiscordEmoji.FromName(ctx.Client, ":mundo:");

                var raceBtnBuilder = new DiscordMessageBuilder();

                raceBtnBuilder = Utiles.crearMensajeDeBotones(
                        ctx,
                        "`[EDAD REGISTRADA: " + nuevaFicha.bird_year + "]` ```3: Elegir-raza:```",
                        new string[] { "HUMANO", "CHARR", "NORN", "ASURA", "SYLVARI" },
                        new string[] { "Humano", "Charr", "Norn", "Asura", "Sylvari" },
                        null,
                        new string[] { ":humano:", ":charr:", ":norn:", ":asura:", ":sylvari:" }
                    );

                await author.SendMessageAsync(raceBtnBuilder).ConfigureAwait(false);

                ctx.Client.ComponentInteractionCreated += async (s, e) =>
                {
                    ///Probamos con una variable para indicar en CUÁL de ambas opciones está. Podrían estar cruzándose los hilos.
                    if (currentButtonsBuilder == 1)
                    {

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("`[RAZA GUARDADA: " + e.Id + "]` ```6: Ocupación-actual.```"));
                        nuevaFicha.raza = (Util.Utiles.Raza)Enum.Parse(typeof(Util.Utiles.Raza), e.Id.ToUpper());

                        switch(nuevaFicha.raza)
                        {
                            case Utiles.Raza.ASURA:
                                color = DiscordColor.Purple;
                                break;
                            case Utiles.Raza.CHARR:
                                color = DiscordColor.Red;
                                break;
                            case Utiles.Raza.NORN:
                                color = DiscordColor.CornflowerBlue;
                                break;
                            case Utiles.Raza.SYLVARI:
                                color = DiscordColor.SapGreen;
                                break;
                        }
                    }
                };
                #endregion

                #region Ocupación
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.ocupacion = interMSG.Content;
                #endregion
                #endregion

                #region DATOS FÍSICOS
                #region Descripción física
                await author.SendMessageAsync("`[OCUPACIÓN GUARDADA: " + nuevaFicha.ocupacion +"]` ```Pasando-a-DATOS-FÍSICOS. 7: Descripción-física.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descripcion_fisica = interMSG.Content;
                #endregion

                #region Altura y peso
                await author.SendMessageAsync("`[DESCRIPCIÓN FÍSICA GUARDADA]` ```8: Altura-aproximada.```").ConfigureAwait(false);
                double altura;
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                while (!Double.TryParse(interMSG.Content, out altura))
                {
                    await author.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                }
                nuevaFicha.altura = altura;
                

                await author.SendMessageAsync("`[ALTURA GUARDADA: " + nuevaFicha.altura + "]` ```9: Peso-aproximado.```").ConfigureAwait(false);
                double peso;
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                while (!Double.TryParse(interMSG.Content, out peso))
                {
                    await author.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                }
                nuevaFicha.peso = peso;
                #endregion

                #region Condición física
                await author.SendMessageAsync("`[PESO GUARDADO: " + nuevaFicha.peso + "]` ```10: Condición-física.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.condicion_fisica = interMSG.Content;
                #endregion

                #region Color de ojos y pelo
                await author.SendMessageAsync("`[CONDICIÓN FÍSICA GUARDADA]` ```11: Color-de-ojos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.color_ojos = interMSG.Content;
                

                await author.SendMessageAsync("`[COLOR DE OJOS GUARDADO: " + nuevaFicha.color_ojos + "]` ```12: Color-de-pelo.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.color_pelo = interMSG.Content;
                #endregion

                #region Rasgos característicos
                await author.SendMessageAsync("`[COLOR DE PELO GUARDADO: " + nuevaFicha.color_pelo + "]` ```13: Rasgos-característicos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.rasgos_caracteristicos = interMSG.Content;
                #endregion
                #endregion

                #region DATOS PSICOLÓGICOS
                #region Descripción psicológica
                await author.SendMessageAsync("`[RASGOS GUARDADOS]` ```Pasando-a-DATOS-PSICOLÓGICOS. 14: Descripción-psicológica.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descripcion_psicologica = interMSG.Content;
                #endregion

                #region Disgustos
                await author.SendMessageAsync("`[DESCRIPCIÓN PSICOLÓGICA GUARDADA]` ```15: Disgustos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.disgustos = interMSG.Content;
                #endregion

                #region Habilidades
                await author.SendMessageAsync("`[DISGUSTOS GUARDADOS]` ```16: Habilidades.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.habilidades = interMSG.Content;
                #endregion

                #region Debilidades
                await author.SendMessageAsync("`[HABILIDADES GUARDADAS]` ```17: Debilidades.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.debilidades = interMSG.Content;
                #endregion
                #endregion

                #region HISTORIA PERSONAL
                await author.SendMessageAsync("`[DEBILIDADES GUARDADAS]` ```Insertar-a-continuación-historia-personal. Puede-ser-texto, un-enlace-a-documento-o-dejarlo-en-blanco.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.historia_personal = interMSG.Content;                
                #endregion

                #region CARACTERÍSTICAS
                await author.SendMessageAsync("`[HISTORIA GUARDADA]` ```Ahora-pasaremos-a-las-características.```").ConfigureAwait(false);

                var muscleEmoji = DiscordEmoji.FromName(ctx.Client, ":muscle:");
                var jugglerEmoji = DiscordEmoji.FromName(ctx.Client, ":juggler:");
                var brainEmoji = DiscordEmoji.FromName(ctx.Client, ":brain:");
                var dancerEmoji = DiscordEmoji.FromName(ctx.Client, ":dancer:");
                var eyeEmoji = DiscordEmoji.FromName(ctx.Client, ":eye:");
                var magic_wandEmoji = DiscordEmoji.FromName(ctx.Client, ":magic_wand:");
                var arrow_down_smallEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_down_small:");
                var white_check_markEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                var xEmoji = DiscordEmoji.FromName(ctx.Client, ":x:");

                await author.SendMessageAsync("```Instrucciones:``` Primero, se-contarán-los-puntos-por-características. Éstas-son-**FUERZA** " + muscleEmoji + ", **DESTREZA** " + jugglerEmoji + ", **INTELIGENCIA** " + brainEmoji + ", **CARISMA** " + dancerEmoji + ", **PERCEPCIÓN** " + eyeEmoji + "-y-**MAGIA** " + magic_wandEmoji + ". Los-puntos-máximos-"
                    + "a-repartir-son-10, pudiendo-dar-comó-máximo-4-a-cada-característica, a-excepción-de-**MAGIA**, donde-el-máximo-serán-3.\nPresionar-cada-icono-hasta-alcanzar-cantidad-deseada-o-máxima. Para-**restar**, pulsar " + arrow_down_smallEmoji + "-y-la-característica-concreta. Puede-haber-números-negativos-en-función-"
                    + "de-las-cualidades-del-personaje. Al-**terminar**, pulsar " + white_check_markEmoji + ". En-caso-de-querer-**reiniciar**, pulsar " + xEmoji + ".").ConfigureAwait(false);

                var caracteristicasMSG = await author.SendMessageAsync("```CONTADOR:``` :muscle: FUERZA: " + nuevaFicha.FUERZA + "\n:juggler: DESTREZA: " + nuevaFicha.DESTREZA + "\n:brain: INTELIGENCIA: " + nuevaFicha.INTELIGENCIA + "\n:dancer: CARISMA: "
                    + nuevaFicha.CARISMA + "\n:eye: PERCEPCION: " + nuevaFicha.PERCEPCION + "\n:magic_wand: MAGIA: " + nuevaFicha.MAGIA + "\n**TOTAL**: " + nuevaFicha.TOTALES + "").ConfigureAwait(false);

                await caracteristicasMSG.CreateReactionAsync(muscleEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(jugglerEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(brainEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(dancerEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(eyeEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(magic_wandEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(arrow_down_smallEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(white_check_markEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(xEmoji).ConfigureAwait(false);

                bool finished = false;
                while (!finished)
                {
                    emoji = null;

                    int addValue = 1;

                    while (emoji == null || !(new[] { muscleEmoji, jugglerEmoji, brainEmoji, dancerEmoji, eyeEmoji, magic_wandEmoji, arrow_down_smallEmoji, white_check_markEmoji, xEmoji }.Contains(emoji)))
                    {
                        if (emoji != null) await author.SendMessageAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-iconos-nombrados.```").ConfigureAwait(false);
                        emoji = (await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == author).ConfigureAwait(false)).Result.Emoji;
                    }

                    string[] msgError = { "", "", "", "", "", "", "" };

                    if (nuevaFicha.TOTALES == 10 && !(new[] { arrow_down_smallEmoji, white_check_markEmoji, xEmoji }.Contains(emoji)))
                    {
                        msgError[msgError.Count() - 1] = " Total-de-puntos-alcanzado. Imposible-añadir-más-puntos.";
                    }
                    else
                    {
                        if (emoji == arrow_down_smallEmoji)
                        {
                            emoji = (await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == author).ConfigureAwait(false)).Result.Emoji;
                            addValue = -1;
                        }

                        if (emoji == muscleEmoji)
                        {
                            if (nuevaFicha.FUERZA < 4 || addValue == -1)
                            {
                                nuevaFicha.FUERZA += addValue;
                            }
                            else
                            {
                                msgError[0] = " Límite-de-fuerza-alcanzado.";
                            }
                        }

                        else if (emoji == jugglerEmoji)
                        {
                            if (nuevaFicha.DESTREZA < 4 || addValue == -1)
                            {
                                nuevaFicha.DESTREZA += addValue;
                            }
                            else
                            {
                                msgError[1] = " Límite-de-destreza-alcanzado.";
                            }
                        }

                        else if (emoji == brainEmoji)
                        {
                            if (nuevaFicha.INTELIGENCIA < 4 || addValue == -1)
                            {
                                nuevaFicha.INTELIGENCIA += addValue;
                            }
                            else
                            {
                                msgError[2] = " Límite-de-inteligencia-alcanzado.";
                            }
                        }

                        else if (emoji == dancerEmoji)
                        {
                            if (nuevaFicha.CARISMA < 4 || addValue == -1)
                            {
                                nuevaFicha.CARISMA += addValue;
                            }
                            else
                            {
                                msgError[3] = " Límite-de-carisma-alcanzado.";
                            }
                        }

                        else if (emoji == eyeEmoji)
                        {
                            if (nuevaFicha.PERCEPCION < 4 || addValue == -1)
                            {
                                nuevaFicha.PERCEPCION += addValue;
                            }
                            else
                            {
                                msgError[4] = " Límite-de-percepción-alcanzado.";
                            }
                        }

                        else if (emoji == magic_wandEmoji)
                        {
                            if (nuevaFicha.MAGIA < 3 || addValue == -1)
                            {
                                nuevaFicha.MAGIA += addValue;
                            }
                            else
                            {
                                msgError[5] = " Límite-de-magia-alcanzado.";
                            }
                        }

                        else if (emoji == xEmoji)
                        {
                            nuevaFicha.FUERZA = 0;
                            nuevaFicha.DESTREZA = 0;
                            nuevaFicha.INTELIGENCIA = 0;
                            nuevaFicha.CARISMA = 0;
                            nuevaFicha.PERCEPCION = 0;
                            nuevaFicha.MAGIA = 0;
                        }

                        else if (emoji == white_check_markEmoji)
                        {
                            finished = true;
                        }
                    }

                    await caracteristicasMSG.ModifyAsync("```CONTADOR:``` :muscle: FUERZA: " + nuevaFicha.FUERZA + msgError[0] + "\n:juggler: DESTREZA: " + nuevaFicha.DESTREZA + msgError[1] + "\n:brain: INTELIGENCIA: "
                        + nuevaFicha.INTELIGENCIA + msgError[2] + "\n:dancer: CARISMA: " + nuevaFicha.CARISMA + msgError[3] + "\n:eye: PERCEPCION: " + nuevaFicha.PERCEPCION + msgError[4] + "\n:magic_wand: MAGIA: "
                        + nuevaFicha.MAGIA + msgError[5] + "\n**TOTAL**: " + nuevaFicha.TOTALES + msgError[6] + "");

                    await caracteristicasMSG.DeleteReactionAsync(arrow_down_smallEmoji, ctx.User).ConfigureAwait(false);
                    await caracteristicasMSG.DeleteReactionAsync(emoji, ctx.User).ConfigureAwait(false);
                }

                await author.SendMessageAsync("`[CONTADOR GUARDADO]` ```Necesarios-descriptores-sobre-cada-característica. Por-ejemplo: \"INTELIGENCIA: Erutido\" o \"DESTREZA: Pistolero\"```").ConfigureAwait(false);
                await author.SendMessageAsync("```¿Fuerza?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorFUERZA = interMSG.Content;
                
                await author.SendMessageAsync("```¿Destreza?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorDESTREZA = interMSG.Content;
                
                await author.SendMessageAsync("```¿Inteligencia?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorINTELIGENCIA = interMSG.Content;
                
                await author.SendMessageAsync("```¿Carisma?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorCARISMA = interMSG.Content;
                
                await author.SendMessageAsync("```¿Percepcion?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorPERCEPCION = interMSG.Content;
                
                await author.SendMessageAsync("```¿Magia?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Author == author && x.Channel.IsPrivate).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorMAGIA = interMSG.Content;
                #endregion

                await author.SendMessageAsync("`[CARACTERÍSTICAS TERMINADAS]` ```Mostrando-resultado-final...```").ConfigureAwait(false);

                var embedFicha = new DiscordEmbedBuilder
                {
                    Title = nuevaFicha.nombre_completo + " " + nuevaFicha.apellidos,
                    Description = nuevaFicha.Mostrar().ToString(),
                    Color = color,
                };

                await author.SendMessageAsync(embed: embedFicha).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Método para la realización de un evento
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public async Task Evento(CommandContext ctx)
        {
            try
            {
                DiscordMember author = ctx.Member;
                Evento nuevoEvento = new Evento();
                DiscordChannel canalEvento = ctx.Channel;
                var interactivity = ctx.Client.GetInteractivity();

                #region Canal de eventos
                var channelMSG = new InteractivityResult<DiscordMessage>();
                await ctx.Channel.SendMessageAsync("```Indicar-nombre-del-canal-de-evento: ```").ConfigureAwait(false);
                channelMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                string channelName = channelMSG.Result.Content;
                canalEvento = Consultas.GetChannel(ctx, channelName);

                //ulong channelID = 0;
                //while (!ulong.TryParse(channelMSG.Result.Content.Remove(0, 2).Replace('>', ' ').Trim(), out channelID) || channelID == 0)
                //{
                //    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Recomendable-citar-canal-en-mensaje-para-facilitar-procesamiento. Inténtelo-de-nuevo:```").ConfigureAwait(false);
                //    channelMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                //}

                //canalEvento = ctx.Guild.GetChannel(channelID);
                #endregion

                #region Nombre
                await ctx.Channel.SendMessageAsync("```Preparando... Nombre-del-evento:```").ConfigureAwait(false);
                nuevoEvento.nombre = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Descripción
                await ctx.Channel.SendMessageAsync("```Descripción-del-evento:```").ConfigureAwait(false);
                nuevoEvento.descripcion = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Fecha y hora
                await ctx.Channel.SendMessageAsync("```Concretar-fecha-y-hora-de-realización. Texto-libre:```").ConfigureAwait(false);
                nuevoEvento.fecha = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Lugar
                await ctx.Channel.SendMessageAsync("```Lugar-del-evento:```").ConfigureAwait(false);
                nuevoEvento.lugar = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Narradores
                await ctx.Channel.SendMessageAsync("```Mencionar-al-narrador-o-los-múltiples-narradores:```").ConfigureAwait(false);
                string[] lstNarradores = ((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content).Trim().Split(',', ' ');
                nuevoEvento.narradores = lstNarradores.ToList<string>();
                #endregion

                #region Participantes
                await ctx.Channel.SendMessageAsync("```Mencionar-a-los-participantes-o-roles-participantes:```").ConfigureAwait(false);
                string[] lstParticipantes = ((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content).Trim().Split(',', ' ');
                nuevoEvento.participantes = lstParticipantes.ToList<string>();
                #endregion

                await ctx.Channel.SendMessageAsync("`[CARACTERÍSTICAS GUARDADAS]` ```Creando-evento-en-canal-correspondiente...```").ConfigureAwait(false);

                var embedFicha = new DiscordEmbedBuilder
                {
                    Title = nuevoEvento.nombre,
                    Description = nuevoEvento.Mostrar().ToString(),
                    Color = DiscordColor.Red
                };

                await canalEvento.SendMessageAsync(embed: embedFicha).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }
    }
}
