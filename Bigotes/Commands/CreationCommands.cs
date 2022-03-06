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
                        var encuestaMSG = await ctx.Channel.SendMessageAsync("`[CREACIÓN DE ENCUESTA ESCOGIDA]` ```Título-de-encuesta-requerido.```").ConfigureAwait(false);
                        await Poll(ctx, encuestaMSG);
                        break;

                    case "FICHA":
                        var msg = await ctx.Channel.SendMessageAsync("`[DESCARGANDO TUTORIAL DE FICHA]` ```De-acuerdo. Procediendo-a-comenzar-ficha-de-personaje-paso-por-paso...```").ConfigureAwait(false);
                        await Task.Delay(2000);
                        await Ficha(ctx, msg);
                        break;

                    case "EVENTO":
                        bool canalEscogido = false;
                        if (Utiles.canalEventos != null)
                        {
                            var yesNoBtnBuilder = Utiles.crearMensajeDeBotones(
                                    ctx,
                                    "`[OPCIÓN ESCOGIDA]` ```Canal-de-eventos-escogido: " + Utiles.canalEventos.Name + ". ¿Desea-cambiar-de-canal?```",
                                    new string[] { "SI", "NO" },
                                    new string[] { "Sí", "No" },
                                    new ButtonStyle[] { ButtonStyle.Success, ButtonStyle.Danger }
                                );

                            ctx.Client.ComponentInteractionCreated += async (s, e) =>
                            {
                                if (e.Id.ToUpper() == "NO") canalEscogido = true;
                            };
                        }
                        
                        if (!canalEscogido)
                        {
                            await ctx.Channel.SendMessageAsync("```Indicar-canal-de-evento: ```").ConfigureAwait(false);
                            var channelMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);

                            ulong channelID = 0;
                            while (!ulong.TryParse(channelMSG.Result.Content.Remove(0, 2).Replace('>', ' ').Trim(), out channelID) || channelID == 0)
                            {
                                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Recomendable-citar-canal-en-mensaje-para-facilitar-procesamiento. Inténtelo-de-nuevo:```").ConfigureAwait(false);
                                channelMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                            }

                            Utiles.canalEventos = ctx.Guild.GetChannel(channelID);
                        }

                        await Evento(ctx, Utiles.canalEventos);
                        break;

                    //case "ROL":
                    //    if (ctx.Member.Roles.Where(x => x.Permissions.HasFlag(Permissions.Administrator)).Count() == 0)
                    //    {
                    //        throw new Exception("No tienes permisos para realizar esta acción.");
                    //    }

                    //    var editMSG = await ctx.Channel.SendMessageAsync("`[REVISANDO LISTA DE ROLES]` ```Un-momento...```").ConfigureAwait(false);
                    //    await Task.Delay(2000);

                    //    await Rol(ctx, editMSG);

                    //    break;

                    default:
                        throw new Exception("Opción de creación no encontrada.");
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx, ex.Message);
            }
        }

        /// <summary>
        /// Método para la realización de encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="titulo"></param>
        /// <returns></returns>
        public async Task Poll(CommandContext ctx, DiscordMessage principalMSG)
        {
            try
            {
                #region Atributos
                TimeSpan duration = new TimeSpan();
                DiscordMember author = ctx.Member;
                DiscordChannel embedChannel = ctx.Channel;
                string titulo = String.Empty;
                string description = String.Empty;
                string numOptions = "0";
                string urlEncuesta = String.Empty;
                List<DiscordEmoji> emojiOptions = new List<DiscordEmoji>();
                var interactivity = ctx.Client.GetInteractivity();
                DiscordMessage interMSG;
                #endregion

                #region Título de la encuesta
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                titulo = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Recogida y gestión de la duración de la encuesta
                await principalMSG.ModifyAsync("`[PROCESANDO PETICIÓN]` ```Orden-recibida.-Preciso-concretar-duración-en-minutos(m),-horas(h)-o-días(d):```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result;
                string durationSTR = interMSG.Content.Trim(), unidadRespuesta;
                int durationINT = 0;

                while (!Regex.IsMatch(durationSTR, @"\d[mhd]"))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-00m,-00h-o-00d.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result;
                    durationSTR = interMSG.Content.Trim();
                }

                if (durationSTR.Contains("m"))
                {
                    durationINT = Int32.Parse(durationSTR.Split('m')[0]);
                    duration = new TimeSpan(0, durationINT, 0);
                    unidadRespuesta = durationINT == 1 ? "minuto" : "minutos";
                }
                else if (durationSTR.Contains("h"))
                {
                    durationINT = Int32.Parse(durationSTR.Split('h')[0]);
                    duration = new TimeSpan(durationINT, 0, 0);
                    unidadRespuesta = durationINT == 1 ? "hora" : "horas";
                }
                else
                {
                    durationINT = Int32.Parse(durationSTR.Split('d')[0]);
                    duration = new TimeSpan(durationINT, 0, 0, 0);
                    unidadRespuesta = durationINT == 1 ? "día" : "días";
                }

                await interMSG.DeleteAsync();
                #endregion

                #region Recogida y gestión del canal en el que realizar la encuesta
                await principalMSG.ModifyAsync("`[PROCESANDO DURACIÓN]` ```Programada-duración-de-" + durationINT + "-" + unidadRespuesta + ".``` ```Detallar-nombre-de-canal-de-encuesta.-Libertad-de-tomar-canal-actual-en-caso-de-no-existir-petición.```").ConfigureAwait(false);

                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result;
                string channelName = interMSG.Content;
                embedChannel = Consultas.GetChannel(ctx, channelName);

                await interMSG.DeleteAsync();
                #endregion

                #region Recogida y gestión de la descripción
                await principalMSG.ModifyAsync("`[BUSCANDO CANAL]` ```Canal-de encuesta-seleccionado:-" + embedChannel.Name + ".``` ```Se-precisa-texto-descriptivo:```").ConfigureAwait(false);

                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result;
                description = interMSG.Content;

                await interMSG.DeleteAsync();
                #endregion

                #region Registro de número de opciones
                await principalMSG.ModifyAsync("`[RECOGIENDO DESCRIPCIÓN]` ```¿Número-de-opciones?```");

                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result;
                numOptions = interMSG.Content;
                while (!Regex.IsMatch(numOptions, @"\d"))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-de-numero-entero-positivo.```").ConfigureAwait(false);
                    numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
                }

                await interMSG.DeleteAsync();
                #endregion

                #region Obtención de emojis para las opciones (reacciones)
                var emojiMSG = await principalMSG.ModifyAsync("`[NÚMERO AJUSTADO]` ```Necesarias-reacciones-a-este-mensaje-con-los-emojis-utilizados-en-cada-opción.```");

                while (emojiOptions.Count < Int32.Parse(numOptions))
                {
                    var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == emojiMSG && x.User == author).ConfigureAwait(false);

                    emojiOptions.Add(reactionResult.Result.Emoji);

                    await ctx.Channel.SendMessageAsync("```Procesada-opción-" + emojiOptions.Count + ":" + reactionResult.Result.Emoji.Name + "```").ConfigureAwait(false);
                }
                #endregion

                #region Creación de encuesta
                await principalMSG.ModifyAsync("`[OPCIONES RECOGIDAS]` ```Se-han-recogido-las-opciones.-Procesando-encuesta...```");

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

                var pollMessage = await embedChannel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

                foreach (var option in emojiOptions)
                {
                    await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
                }

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

                //results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

                var msgFinal = "`ENCUESTA " + titulo + " FINALIZADA` ```Resultados:```";

                await pollMessage.RespondAsync(msgFinal + string.Join("\n", results));

                //await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
                #endregion
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx, ex.Message);
            }
        }

        /// <summary>
        /// Método para la realización de una ficha de personaje
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public async Task Ficha(CommandContext ctx, DiscordMessage principalMSG)
        {
            try
            {
                #region Atributos
                Ficha nuevaFicha = new Ficha();
                DiscordEmoji emoji = null;
                var interactivity = ctx.Client.GetInteractivity();
                DiscordMessage interMSG;
                DiscordColor color = DiscordColor.Yellow;
                int currentButtonsBuilder = 0; //Para controlar en qué mensaje de botones se encuentra
                #endregion

                #region DATOS BÁSICOS
                #region Nombre y apellidos
                await principalMSG.ModifyAsync("`[OPCIÓN ESCOGIDA]` ```Ficha-en-blanco-preparada. Comenzando-por-DATOS-BÁSICOS. 1: Nombre.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.nombre_completo = interMSG.Content;
                await interMSG.DeleteAsync();

                await principalMSG.ModifyAsync("`[NOMBRE GUARDADO: " + nuevaFicha.nombre_completo + "]` ```2: Apellidos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.apellidos = interMSG.Content;
                await interMSG.DeleteAsync();
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

                await principalMSG.ModifyAsync(genderBtnBuilder).ConfigureAwait(false);

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
                //await principalMSG.ModifyAsync("```4: Año-de-nacimiento.```").ConfigureAwait(false);
                int anio;
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                while (!Int32.TryParse(interMSG.Content, out anio))
                {
                    await principalMSG.ModifyAsync("`[ERROR]` ```Insertar-número-válido-para-el-año-de-nacimiento.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                }
                nuevaFicha.bird_year = anio;
                await interMSG.DeleteAsync();
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

                await principalMSG.ModifyAsync(raceBtnBuilder).ConfigureAwait(false);

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
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.ocupacion = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion
                #endregion

                #region DATOS FÍSICOS
                #region Descripción física
                await principalMSG.ModifyAsync("`[OCUPACIÓN GUARDADA: " + nuevaFicha.ocupacion +"]` ```Pasando-a-DATOS-FÍSICOS. 7: Descripción-física.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descripcion_fisica = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Altura y peso
                await principalMSG.ModifyAsync("`[DESCRIPCIÓN FÍSICA GUARDADA]` ```8: Altura-aproximada.```").ConfigureAwait(false);
                double altura;
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                while (!Double.TryParse(interMSG.Content, out altura))
                {
                    await principalMSG.ModifyAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                }
                nuevaFicha.altura = altura;
                await interMSG.DeleteAsync();

                await principalMSG.ModifyAsync("`[ALTURA GUARDADA: " + nuevaFicha.altura + "]` ```9: Peso-aproximado.```").ConfigureAwait(false);
                double peso;
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                while (!Double.TryParse(interMSG.Content, out peso))
                {
                    await principalMSG.ModifyAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                    interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                }
                nuevaFicha.peso = peso;
                await interMSG.DeleteAsync();
                #endregion

                #region Condición física
                await principalMSG.ModifyAsync("`[PESO GUARDADO: " + nuevaFicha.peso + "]` ```10: Condición-física.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.condicion_fisica = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Color de ojos y pelo
                await principalMSG.ModifyAsync("`[CONDICIÓN FÍSICA GUARDADA]` ```11: Color-de-ojos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.color_ojos = interMSG.Content;
                await interMSG.DeleteAsync();

                await principalMSG.ModifyAsync("`[COLOR DE OJOS GUARDADO: " + nuevaFicha.color_ojos + "]` ```12: Color-de-pelo.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.color_pelo = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Rasgos característicos
                await principalMSG.ModifyAsync("`[COLOR DE PELO GUARDADO: " + nuevaFicha.color_pelo + "]` ```13: Rasgos-característicos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.rasgos_caracteristicos = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion
                #endregion

                #region DATOS PSICOLÓGICOS
                #region Descripción psicológica
                await principalMSG.ModifyAsync("`[RASGOS GUARDADOS]` ```Pasando-a-DATOS-PSICOLÓGICOS. 14: Descripción-psicológica.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descripcion_psicologica = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Disgustos
                await principalMSG.ModifyAsync("`[DESCRIPCIÓN PSICOLÓGICA GUARDADA]` ```15: Disgustos.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.disgustos = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Habilidades
                await principalMSG.ModifyAsync("`[DISGUSTOS GUARDADOS]` ```16: Habilidades.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.habilidades = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region Debilidades
                await principalMSG.ModifyAsync("`[HABILIDADES GUARDADAS]` ```17: Debilidades.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.debilidades = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion
                #endregion

                #region HISTORIA PERSONAL
                await principalMSG.ModifyAsync("`[DEBILIDADES GUARDADAS]` ```Insertar-a-continuación-historia-personal. Puede-ser-texto, un-enlace-a-documento-o-dejarlo-en-blanco.```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.historia_personal = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                #region CARACTERÍSTICAS
                await principalMSG.ModifyAsync("`[HISTORIA GUARDADA]` ```Ahora-pasaremos-a-las-características.```").ConfigureAwait(false);

                var muscleEmoji = DiscordEmoji.FromName(ctx.Client, ":muscle:");
                var jugglerEmoji = DiscordEmoji.FromName(ctx.Client, ":juggler:");
                var brainEmoji = DiscordEmoji.FromName(ctx.Client, ":brain:");
                var dancerEmoji = DiscordEmoji.FromName(ctx.Client, ":dancer:");
                var eyeEmoji = DiscordEmoji.FromName(ctx.Client, ":eye:");
                var magic_wandEmoji = DiscordEmoji.FromName(ctx.Client, ":magic_wand:");
                var arrow_down_smallEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_down_small:");
                var white_check_markEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                var xEmoji = DiscordEmoji.FromName(ctx.Client, ":x:");

                await principalMSG.ModifyAsync("```Instrucciones:``` Primero, se-contarán-los-puntos-por-características. Éstas-son-**FUERZA** " + muscleEmoji + ", **DESTREZA** " + jugglerEmoji + ", **INTELIGENCIA** " + brainEmoji + ", **CARISMA** " + dancerEmoji + ", **PERCEPCIÓN** " + eyeEmoji + "-y-**MAGIA** " + magic_wandEmoji + ". Los-puntos-máximos-"
                    + "a-repartir-son-10, pudiendo-dar-comó-máximo-4-a-cada-característica, a-excepción-de-**MAGIA**, donde-el-máximo-serán-3.\nPresionar-cada-icono-hasta-alcanzar-cantidad-deseada-o-máxima. Para-**restar**, pulsar " + arrow_down_smallEmoji + "-y-la-característica-concreta. Puede-haber-números-negativos-en-función-"
                    + "de-las-cualidades-del-personaje. Al-**terminar**, pulsar " + white_check_markEmoji + ". En-caso-de-querer-**reiniciar**, pulsar " + xEmoji + ".").ConfigureAwait(false);

                var caracteristicasMSG = await ctx.Channel.SendMessageAsync("```CONTADOR:``` :muscle: FUERZA: " + nuevaFicha.FUERZA + "\n:juggler: DESTREZA: " + nuevaFicha.DESTREZA + "\n:brain: INTELIGENCIA: " + nuevaFicha.INTELIGENCIA + "\n:dancer: CARISMA: "
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
                        if (emoji != null) await principalMSG.ModifyAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-iconos-nombrados.```").ConfigureAwait(false);
                        emoji = (await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
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
                            emoji = (await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
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

                await principalMSG.ModifyAsync("`[CONTADOR GUARDADO]` ```Necesarios-descriptores-sobre-cada-característica. Por-ejemplo: \"INTELIGENCIA: Erutido\" o \"DESTREZA: Pistolero\"```").ConfigureAwait(false);
                await caracteristicasMSG.ModifyAsync("```¿Fuerza?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorFUERZA = interMSG.Content;
                await interMSG.DeleteAsync();
                await caracteristicasMSG.ModifyAsync("```¿Destreza?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorDESTREZA = interMSG.Content;
                await interMSG.DeleteAsync();
                await caracteristicasMSG.ModifyAsync("```¿Inteligencia?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorINTELIGENCIA = interMSG.Content;
                await interMSG.DeleteAsync();
                await caracteristicasMSG.ModifyAsync("```¿Carisma?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorCARISMA = interMSG.Content;
                await interMSG.DeleteAsync();
                await caracteristicasMSG.ModifyAsync("```¿Percepcion?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorPERCEPCION = interMSG.Content;
                await interMSG.DeleteAsync();
                await caracteristicasMSG.ModifyAsync("```¿Magia?```").ConfigureAwait(false);
                interMSG = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result;
                nuevaFicha.descriptorMAGIA = interMSG.Content;
                await interMSG.DeleteAsync();
                #endregion

                await principalMSG.ModifyAsync("`[CARACTERÍSTICAS TERMINADAS]` ```Mostrando-resultado-final...```").ConfigureAwait(false);

                var embedFicha = new DiscordEmbedBuilder
                {
                    Title = nuevaFicha.nombre_completo + " " + nuevaFicha.apellidos,
                    Description = nuevaFicha.Mostrar().ToString(),
                    Color = color,
                };

                await ctx.Channel.SendMessageAsync(embed: embedFicha).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx, ex.Message);
            }
        }

        /// <summary>
        /// Método para la realización de un evento
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="canalEncuesta"></param>
        /// <returns></returns>
        public async Task Evento(CommandContext ctx, DiscordChannel canalEncuesta)
        {
            try
            {
                Evento nuevoEvento = new Evento();
                var interactivity = ctx.Client.GetInteractivity();

                #region Nombre
                await ctx.Channel.SendMessageAsync("```Preparando... Nombre-del-evento:```").ConfigureAwait(false);
                nuevoEvento.nombre = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Descripción
                await ctx.Channel.SendMessageAsync("```Descripción-del-evento:```").ConfigureAwait(false);
                nuevoEvento.descripcion = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                //REVISAR FECHA Y HORA EN PRUEBAS
                #region Fecha y hora
                await ctx.Channel.SendMessageAsync("```Concretar-fecha-y-hora-de-realización-con-formato-dd/mm/yyyy hh:mm:```").ConfigureAwait(false);
                DateTime fechaAux = new DateTime();

                var dateMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                while (!DateTime.TryParse(dateMSG.Result.Content, out fechaAux))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Formato-incorrecto. Recomendado-formato-coherente-dd/mm/yyyy hh:mm").ConfigureAwait(false);
                    dateMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                }

                nuevoEvento.fecha = fechaAux;
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

                await canalEncuesta.SendMessageAsync(embed: embedFicha).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx, ex.Message);
            }
        }
    }
}
