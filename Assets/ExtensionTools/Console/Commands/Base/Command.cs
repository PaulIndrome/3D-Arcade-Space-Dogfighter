using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    abstract public class Command
    {

        private Parameter[] m_Parameters=new Parameter[] { };

        protected Command(Parameter[] parameters=null)
        {
            if (parameters == null)
                parameters = new Parameter[] { };
            m_Parameters = parameters;
        }

        public Parameter[] Parameters {
            get => m_Parameters;
        }

        public bool TryParse(string[] stringparameters,out object[] parsedparameters,out int ErrorParameter) {
            parsedparameters = new object[stringparameters.Length];

            ErrorParameter = -1;
            if (stringparameters.Length != Parameters.Length)
                return false;

            for (int i = 0; i < Parameters.Length; i++) {
                object parsedparameter;
                if (!Parameters[i].TryParse(stringparameters[i],out parsedparameter))
                {
                    ErrorParameter = i;
                    return false;
                }
                else
                {
                    parsedparameters[i] = parsedparameter;
                }
            }

            return true;
        }
        public abstract string GetCommand();
        public abstract string GetHelpString();
        public string GetErrorParsingString(int ParameterIndex) {
            if (ParameterIndex == -1)
            {
                return "Wrong amount of parameters! INFO: " + GetHelpString();
            }



            return "Parameter " + Parameters[ParameterIndex].GetType().Name.Replace("Parameter","") + " is not valid!";
        }

        public abstract string Execute(object[] parameters);
    }
}
