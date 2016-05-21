using System;
using System.Collections.Generic;
using System.Text;


namespace FormulaResolve
{
    //define type of input char
    enum charType
    {
        Element,
        Number,
        Operator,
        None
    }

    class SimpleResolver
    {
        public Dictionary<string, float> elementDictionary = new Dictionary<string, float>();   //all registed element and its mass
        private Stack<float> operateNumberStack = new Stack<float>();    // stack for number
        private Stack<char> operatorStack = new Stack<char>(); //stack for operator
        private Stack<int> operatorIndex = new Stack<int>(); // stack for record the upper bound of operater function
        private string tempString = null;

        //convert char to charType
        public static charType getCharType(char inputChar)
        {
            if ((inputChar < 'Z') && (inputChar > 'A'))
                return charType.Element;
            else if (Char.IsNumber(inputChar))
            {
                return charType.Number;
            }
            else if ((inputChar == '(') || (inputChar == ')') || (inputChar == '[') || (inputChar == ']'))
            {
                return charType.Operator;
            }
            else
                return charType.None;
        }

        // get mass from formula string
        public float solveString(string input)
        {
            tempString = input.ToUpper() + "%";  // add the stop point
            int readIndex = 1;  // record read position in this round
            float newMass = 0.0f; // temp for mass calculation
            charType tempCharType = getCharType(tempString[0]);    //current read char type
            string block = null;  // meaningful block for calculation

            do
            {
                readIndex = 0;
                do
                {
                    block = tempString.Substring(0, readIndex + 1);   //get current char block
                    tempCharType = getCharType(tempString[readIndex]);   //get current char type
                    if (charType.Operator == tempCharType)
                    {
                        readIndex++;
                        break;
                    }
                    if ((charType.Element == tempCharType) && (elementDictionary.ContainsKey(block)))  // if block is recorded in dictionary
                    {
                        if (elementDictionary.ContainsKey(tempString.Substring(0, readIndex + 2))) // especially for case like both of Fe and F were registed
                        {
                            readIndex += 2;
                            block = tempString.Substring(0, readIndex);
                            
                            break;
                        }
                        else
                        {
                            readIndex++;
                            break;
                        }
                    }else if (charType.None == tempCharType)
                    {
                        Console.Write("Unsolveable symbol dectected!\n");
                        clearAll();
                        return -1.0f;
                    }
                } while (getCharType(tempString[++readIndex]) == tempCharType);   // if the next char type same as current one?

                switch (tempCharType)
                {
                    case charType.Element:
                        if (elementDictionary.ContainsKey(block))
                        {
                            operateNumberStack.Push(elementDictionary[block]);   // if block is registed as element, push its mass to stack
                        }
                        else
                        {
                            clearAll();  //before return, clear data in stacks
                            Console.Write("Can not find this element {0} in dictionary.\n", block);   
                            return -1.0f;
                        }
                        break;
                    case charType.Number:
                        if (operateNumberStack.Count < 1)
                        {
                            Console.Write("Unsolveable Formula.\n");      // number should follow an element
                            return -1.0f;
                        }
                        newMass = operateNumberStack.Pop() * float.Parse(block);    // calculate with the top of number stack
                        operateNumberStack.Push(newMass);
                        break;
                    case charType.Operator: 
                        if (((')' == block[0]) && ('(' == operatorStack.Peek())) || (']' == block[0]) && ('[' == operatorStack.Peek()))
                        {
                            operatorStack.Pop();
                            sumStackTo(operatorIndex.Pop());
                        }
                        else
                        {
                            operatorStack.Push(block[0]);
                            operatorIndex.Push(operateNumberStack.Count);
                        }

                        break;
                    default:
                        break;
                }
                tempString = tempString.Substring(readIndex);
            } while (tempString.Length > 1);
            sumStackTo(0);
            Console.WriteLine("{0} = {1}", input, operateNumberStack.Peek());
            operatorStack.Clear();
            return operateNumberStack.Pop();
        }
        // sum top of number stack to Count of index
        private bool sumStackTo(int index)
        {
            if (index >= (operateNumberStack.Count))
                return false;
            else
            {
                while (operateNumberStack.Count > (index + 1))
                {
                    if (operateNumberStack.Count == 1)
                        return true;
                    float temp = operateNumberStack.Pop() + operateNumberStack.Pop();
                    operateNumberStack.Push(temp);
                }
                return true;
            }

        }
        // clear stack
        private void clearAll()
        {
            operatorStack.Clear();
            operatorIndex.Clear();
            operateNumberStack.Clear();
            tempString = null;
        }
    }
    class Program
    {
        

        static void Main(string[] args)
        {


            SimpleResolver resolver = new SimpleResolver();

            Console.Write("Please enter standard atomic mass or formula need to be calculate:\n");
            string input = "";
            while(true){
                input = Console.ReadLine();
                if ("END" == input.ToUpper())
                    break;
                if (input.Contains("="))
                {
                    string elementName = input.Substring(0, input.IndexOf('=')).Trim();
                    if (resolver.elementDictionary.ContainsKey(elementName.ToUpper()))
                    {
                        Console.Write("{0} is already in element Dictionary\n",elementName);
                    }
                    else
                    {
                        string elementMass = input.Substring(input.LastIndexOf('=') + 1).Trim();
                        resolver.elementDictionary.Add(elementName.ToUpper(), float.Parse(elementMass));
                        Console.Write("{0} has already been added to element dictionary with mass of {1}\n", elementName, elementMass);
                    }
                }
                else
                {
                    resolver.solveString(input);
                }
            }
        }
    }
}
