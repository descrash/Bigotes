using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Bigotes.Util
{
    /// <summary>
    /// Clase para el cálculo de las tiradas de dados
    /// </summary>
    public class Dices
    {
        private Random rand { get; set; }
        private int[] diceNumber { get; set; }
        private int[] diceRoll { get; set; }
        private int[] diceValue { get; set; }
        string operadores { get; set; }
        string[] operandos { get; set; }
        private int totalValue { get; set; }

        /// <summary>
        /// Método para procesar la cadena de entrada
        /// y realizar la tirada.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int Roll(string input)
        {
            totalValue = 0;
            bool esResta = false;
            string[] subStr;

            try
            {
                rand = new Random();

                #region Comprobación de formato
                if (!Regex.IsMatch(input.Trim(), @"[1-9]\d?d[1-9]\d?(([+-][([1-9]\d?d[1-9]\d?)([1-9]\d?)])*"))
                {
                    throw new Exception("Error en el formato de entrada.");
                }
                #endregion

                #region Obtención de operadores
                foreach (char c in input.Trim())
                {
                    if(c == '+' || c == '-')
                    {
                        operadores += c;
                    }
                }
                operandos = input.Trim().Split('+', '-');
                #endregion

                diceNumber = new int[operandos.Length];
                diceRoll = new int[operandos.Length];
                diceValue = new int[operandos.Length];

                #region Obtención de operandos y operación
                for (int i = 0; i < operandos.Length; i++)
                {
                    subStr = operandos[i].Split('d');
                    
                    if (subStr.Length > 1)
                    {
                        diceNumber[i] = Int32.Parse(subStr[0]);
                        diceRoll[i] = Int32.Parse(subStr[1]);

                        diceValue[i] = diceNumber[i] * (rand.Next(1, diceRoll[i]));
                    }
                    else
                    {
                        diceValue[i] = Int32.Parse(subStr[0]);
                    }

                    if (i != 0)
                    {
                        if (operadores[i-1] == '-')
                        {
                            totalValue -= diceValue[i];
                            esResta = true;
                        }
                    }

                    if (!esResta)
                    {
                        totalValue += diceValue[i];
                    }
                    #endregion
                }
            }
            catch(Exception ex)
            {
                Debug.Write("ERROR: " + ex.Message);
                totalValue = -1;
            }

            return totalValue;
        }
    }
}
