using System;
using System.Configuration;
using System.Collections.Generic;

public class MasterMindMain
{
    private static int NumOfTries = 0;
    private static int CodeLength = 0;
    private static string WinMessage = "";
    private static string LoseMessage = "";

    public static void Main()
    {
        Initialize();

        bool play = true;

        do
        {
            PlayGame(NumOfTries);

            Console.Write("Press Y to play again, N to quit: ");
            Console.Write("\n");
            ConsoleKeyInfo key;
           
            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.N)
                {
                    play = false;
                }
            } while (!(key.Key == ConsoleKey.N || key.Key == ConsoleKey.Y));
                              
        } while (play);

        Environment.Exit(1);
    }

    
    //Function to initialize values that are needed for the game.  It will first try to retrieve the data from the app.config, but if the file is missing or has incorrect data, we'll set default values.  
    public static void Initialize()
    {
        try
        {
            NumOfTries = Convert.ToInt32(ConfigurationManager.AppSettings.Get("NumOfTries"));
            CodeLength = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CodeLength"));
            WinMessage = ConfigurationManager.AppSettings.Get("WinMessage");
            LoseMessage = ConfigurationManager.AppSettings.Get("LoseMessage");

            if (NumOfTries < 1 || CodeLength < 1 || WinMessage is null || LoseMessage is null)
            {
                throw new Exception("One or more properties was loaded with invalid values.");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failure when accessing configuration file: " + ex.Message + "\n" + "Application will use default values." + "\n");
            NumOfTries = GetNumofTries();
            CodeLength = 4;
            WinMessage = "You solved it!";
            LoseMessage = "You Lose :(";
        }
        
    }

    //This funciton will prompt the user for how many attempts they're allowed to guess if that value isn't retrieved from the config.  
    public static int GetNumofTries()
    {
        int returnTries = 0;
        String userInput = "";
        Console.Write("Please manually enter number of tries: ");
        userInput = Console.ReadLine();
        returnTries = Convert.ToInt32(userInput);
        return returnTries;
    }


    //Main game function that calls the methods to retrieve the user and game generated codes, then compares and outputs the result.  
    public static void PlayGame(int NumOfTries)
    {
        int turnNumber = 1;
        String Result = "";
        String Code = GetGeneratedCode();
        do
        {      
            String UserInput = GetUserInput(turnNumber,NumOfTries);
            Result = ValidateResult(UserInput, Code);
            Console.WriteLine("Result: " + Result);
            turnNumber++;

        } while (turnNumber <= NumOfTries && !(Result == WinMessage));

        if ((Result != WinMessage))
        {
            Console.WriteLine(LoseMessage);
            Console.WriteLine("Code was " + Code);
        }
    }



    //Function to generate a random codee for the user to guess.  
    public static string GetGeneratedCode()
    {
        string Code = "";
        Random DigitGenerator = new Random();
        for (int x = 0; x < CodeLength; x++)
        {
            Code += Convert.ToString(DigitGenerator.Next(1, 7));
        }
        return Code;
    }

    //This function will prompt the user to input their guess for what the code is.
    //They will only be able to enter a numeric value 1-6, but cannot proceed until it is 4 digits.  
    public static string GetUserInput(int TurnNumber, int NumOfTries)
    {
        String UserInput = "";
        Console.Write("Turn " + TurnNumber+ " of " + NumOfTries + ". Enter the code: ");
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (!(key.Key == ConsoleKey.Backspace))
            {
                
                if (char.IsDigit(key.KeyChar) && UserInput.Length < CodeLength)   //Prevents typing a digit more than 4 characters, and one that's not numeric.  
                {
                    int number = int.Parse(key.KeyChar.ToString());
                    bool InValidRange = number > 0 && number < 7;

                    if (InValidRange)
                    {
                        UserInput += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                    
                }
            }else
            {
                if (UserInput.Length >= 1)
                {
                    UserInput = UserInput.Remove(UserInput.Length - 1);
                    Console.Write("\b \b");
                }
            }
        } while (key.Key != ConsoleKey.Enter || UserInput.Length < CodeLength); //Second condition ensures the user can't continue without a 4 digit code.  

        Console.Write("\n");
        return UserInput;

    }

    //This funcition compares the user entered code with the one generated by the game, and determines where there's a match.  
    public static string ValidateResult(string UserInput, string Code)
    {
        String Answer = "";

        List<int> PositionsToRemove = new List<int>();

        if (UserInput == Code)  //If the two values are the same, the user wins and we won't have to compare the other values.  
        {
            Answer = WinMessage;
        }else
        {
            for (int x = 0; x < CodeLength; x++) //First Checking Where the numbers are in the same position
            {
                if (UserInput.Substring(x,1) == Code.Substring(x,1))
                {
                    Answer += "+";
                    PositionsToRemove.Add(x);

                }
            }

            //Next remove the positions that were already confirmed correct so that we can check correct numbers in wrong positions.
            //Going in reverse ordere to prevent hitting an index out of range.  
            PositionsToRemove.Reverse();
            foreach (int position in PositionsToRemove)
            {
                UserInput = UserInput.Remove(position, 1);
                Code = Code.Remove(position, 1);
            }

            //Finally we can check if there are matching values in mismatched positions.  
            if (UserInput.Length > 1)
            {
                for (int x = UserInput.Length - 1; x >=0; x--)
                {
                    string digit = UserInput.Substring(x, 1);
                    if (Code.Contains(digit))
                    {
                        UserInput = UserInput.Remove(x,1);
                        Code = Code.Remove(Code.IndexOf(digit),1);
                        Answer += "-";
                    }

                }
            }
            
        }
        return Answer;
    }

}
