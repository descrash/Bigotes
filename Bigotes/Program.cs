using Bigotes.Util;
using System;

namespace Bigotes
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Ajustes de propiedades iniciales
            Properties.cordialidad = 50;
            #endregion

            Bot bot = new Bot();

            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
