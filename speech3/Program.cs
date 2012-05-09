using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;

namespace speech3
{
    class Program
    {
        private static SpeechRecognitionEngine engine;
        private static SpeechSynthesizer speech;

        static void Main(string[] args)
        {
            //initialize synthesizer
            speech = new SpeechSynthesizer();

            //setup the speech engine to listen to our microphone
            engine = new SpeechRecognitionEngine();
            engine.SetInputToDefaultAudioDevice();

            WriteLine("I'm listening...");

            WriteLine("Say 'Hello Computer'");
            //this listens for a specific phrase - in this case
            //'hello computer'
            var phraseGrammar = new GrammarBuilder("Hello Computer");
            ListenForResult(phraseGrammar);

            WriteLine("What is your favorite Red, Blue, or Green color?");
            //GrammarBuilder used to construct phrases for the engine to detect
            var simpleChoiceGrammar = new GrammarBuilder();
            //create an array of words the user could say
            var choices = new Choices("red", "green", "blue");
            simpleChoiceGrammar.Append(choices);
            ListenForResult(simpleChoiceGrammar);

            WriteLine("What is your favorite Red, Blue, or Green color?");
            var mappedChoiceGrammar = new GrammarBuilder("color");
            var choices2 = new Choices();
            //add each word individually as a choice, with a corresponding value
            //we can test for
            choices2.Add(new SemanticResultValue("red", 1));
            choices2.Add(new SemanticResultValue("green", 2));
            choices2.Add(new SemanticResultValue("blue", 3));
            //associate the value of this choice with a "key" we can look for
            mappedChoiceGrammar.Append(new SemanticResultKey("color", choices2));

            //because we appended to the original grammar builder of "color",
            //the user has to say "color red" in order for this to work
            var mappedChoiceResult = ListenForResult(mappedChoiceGrammar);

            //test if result contains our semantic key
            if (mappedChoiceResult.Semantics.ContainsKey("color"))
            {
                //get value from SemanticResultValue
                var color = (int)mappedChoiceResult.Semantics["color"].Value;
                var colorText = "";
                switch (color)
                {
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Red;
                        colorText = "Red";
                        break;
                    case 2:
                        Console.ForegroundColor = ConsoleColor.Green;
                        colorText = "Green";
                        break;
                    case 3:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        colorText = "Blue";
                        break;
                }
                WriteLine(colorText + " is my favorite too!");
            }

            WriteLine("What is your name?");
            var wildcardGrammar = new GrammarBuilder("My name is");
            var wildcard = new GrammarBuilder();
            //append a "wildcard", which is a placeholder for a phrase that
            //we do not decode
            wildcard.AppendWildcard();
            wildcardGrammar.Append(new SemanticResultKey("name", wildcard));
            var wildcardResult = ListenForResult(wildcardGrammar);

            if (wildcardResult.Semantics.ContainsKey("name"))
            {
                //note that wildcard will not actually return anything
                //for a value... it's only a placeholder
                var name = (string)wildcardResult.Semantics["name"].Value;
                WriteLine("Hi " + name + "!");
            }

            WriteLine("What is your name?");
            var dictationGrammar = new GrammarBuilder("My name is");
            var dictation = new GrammarBuilder();
            //append a "dictation", which is a placeholder for a phrase
            //that the speech engine attempts to decode to a string
            dictation.AppendDictation();
            dictationGrammar.Append(new SemanticResultKey("name", dictation));
            var dictationResult = ListenForResult(dictationGrammar);

            if(dictationResult.Semantics.ContainsKey("name"))
            {
                //this will have the best guess at what the user said
                //usually with hilarious results
                var name = (string)dictationResult.Semantics["name"].Value;
                WriteLine("Hi " + name + "!");
            }

            WriteLine("Done listening");

            Console.ReadLine();
        }

        private static RecognitionResult ListenForResult(GrammarBuilder gb)
        {
            engine.LoadGrammar(new Grammar(gb));
            RecognitionResult result = null;
            while(result == null) {
                result = engine.Recognize();
            }

            WriteLine("I'm " + result.Confidence*100 + "% sure you just said " + result.Text);
            //clear out grammars so we can set a new one later
            engine.UnloadAllGrammars();
            return result;
        }

        private static void WriteLine(string input)
        {
            Console.WriteLine(input);
            
            var prompt = new PromptBuilder();
            //build a prompt with a specific string to translate to speech
            //note there's a bunch of overloads to control the volume, rate
            //and other speech factors
            prompt.AppendText(input);

            speech.Speak(prompt);
        }
    }
}
