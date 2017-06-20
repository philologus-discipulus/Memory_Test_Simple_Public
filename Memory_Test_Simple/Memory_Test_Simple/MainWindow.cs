/*
 *Coding Style Standards:
 * FunctionNames and ClassNames
 * localVariables and classAttributes
 * global_variables
*/

using Gtk;
using Newtonsoft.Json;
using UI = Gtk.Builder.ObjectAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

public partial class MainWindow: Gtk.Window {
	
	Builder builder;
	
	static String[] rawWords = ImportWords("words.json");

	// which twelve words are used for the twelve words test 
	String[] wordsTwelve = GetWords(rawWords.Where (word => word.Length >= 3 && word.Length <= 8).ToArray(), 12);

	// stores the twelve answers given by the user
	String[] answersWordsTwelve = new String[12];

	// which of the twelve words for the Twelve Word Test is currently being displayed
	int currentWordsTwelve = 0;

	// the number of responses that match the original twelve words
	int numberCorrectTwelve = 0;

	[UI] Gtk.Button okSubmit;
	[UI] Gtk.Label messageLabel;
	[UI] Gtk.Label notesLabel;
	[UI] Gtk.Label displayLabel;
	[UI] Gtk.Grid gridWords;
	[UI] Gtk.Entry word1;
	[UI] Gtk.Entry word2;
	[UI] Gtk.Entry word3;
	[UI] Gtk.Entry word4;
	[UI] Gtk.Entry word5;
	[UI] Gtk.Entry word6;
	[UI] Gtk.Entry word7;
	[UI] Gtk.Entry word8;
	[UI] Gtk.Entry word9;
	[UI] Gtk.Entry word10;
	[UI] Gtk.Entry word11;
	[UI] Gtk.Entry word12;
	[UI] Gtk.CheckButton enableLogging;



	public static MainWindow Create (){
		
		Builder builder = new Builder (null, "Memory_Test_Simple.interfaces.MainWindow.ui", null);
		return new MainWindow (builder, builder.GetObject ("window1").Handle);
	}

	protected MainWindow (Builder builder, IntPtr handle) : base (handle){
		
		this.builder = builder;

		this.TypeHint = Gdk.WindowTypeHint.Dialog;

		builder.Autoconnect (this);

		okSubmit.Clicked += OnButtonClick;
		this.DeleteEvent += OnDeleteEvent;
	}

	// starts the testing portion of the app after the user acknowledges the notice
	protected void OnButtonClick (object sender, EventArgs a){
		// reconfigure UI
		messageLabel.Visible = false;
		enableLogging.Visible = false;
		notesLabel.Visible = false;

		// give instructions to the end user
		displayLabel.Text = String.Join("Let's test your memory.  We'll give you 12 words and 2.5 seconds",
			" to memorize each word.  After that, we'll show you 12 text boxes, and you can enter",
			" as many of the 12 words as you can remember.  The case and the order of the words",
			" do not matter.  Click 'Ok' to begin.");

		// associate the start function and only the start function with the ok button
		okSubmit.Clicked -= OnButtonClick;
		okSubmit.Clicked += OnBeginTest;
	}

	// takes in the answers given by the user, normalizes them, and records the correct number of answers
	// logging as Save Results? Case and Order don't matter Correct Answers, Incorrect Answers, Words Not Guessed
	protected void EvaluateAnswers(object sender, EventArgs a){
		// have to typecast first since gridWords.Children are treated as Widgets
		answersWordsTwelve = gridWords.Children.Select (word => (Gtk.Entry)word).Select(word => word.Text).ToArray();
		SetVisibleForAllTextBoxes (false);
		SetTextForAllTextBoxes (String.Empty);
		// responses given
		String[] answers = answersWordsTwelve.Select (word => word.Trim ().ToLower ()).Where(word => word != String.Empty).ToArray();
		// words displayed
		String[] prompts = wordsTwelve.Select (word => word.Trim ().ToLower ()).ToArray();
		// correct responses
		String[] correctAnswers = answers.Intersect (prompts).ToArray ();
		// incorrect responses
		String[] incorrectAnswers = answers.Except (prompts).ToArray();
		// words not guessed
		String[] notGuessed = prompts.Except (answers).ToArray();
		numberCorrectTwelve = correctAnswers.Count ();
		// inform the user of their results
		displayLabel.Text = String.Format ("You got {0} words correct.{1}{1}"
			+ "Correct Answers:  {1}{2}{1}{1}Incorrect Answers:  {1}{3}{1}{1}Words Not Guessed:  {1}{4}{1}{1}  Press ok to continue."
			, numberCorrectTwelve, Environment.NewLine, String.Join(", ", correctAnswers) 
			, String.Join(", ", incorrectAnswers), String.Join(", ", notGuessed));
		okSubmit.Clicked -= EvaluateAnswers;
		okSubmit.Clicked += LogResultsIfChecked;
	}

	// sampleSize must be less than or equal to the Length of words after the Selector has been applied
	protected static String[] GetWords(String[] words, Int64 sampleSize){
		// error handling in case there aren't enough words
		if (words.Length < sampleSize) {
			throw new Exception ("Not enough words in sample set.");
		} else if (sampleSize == words.Length) {
			return words;
		} else {
			String[] wordsSelected = new String[sampleSize];
			Random randomNumberGenerator = new Random (Convert.ToInt32(DateTime.Now.ToString("yyyymmddhh")));
			Int32 top = words.Length + 1;
			int currentSelection;
			int currentAddition = 0;

			// shift spots to efficiently sample random words
			do {
				currentSelection = randomNumberGenerator.Next (0, top);
				wordsSelected[currentAddition] = words [currentSelection];
				words [currentSelection] = words [top - 2];
				words [top - 2] = wordsSelected[currentAddition];
				currentAddition ++;
				top --;


			} while(currentAddition < sampleSize);

			return wordsSelected;
		}
	}

	// grab the JSON words object and import the words into a String array
	protected static String[] ImportWords(String fileName){
		using(StreamReader file = new StreamReader(String.Format("input/{0}", fileName))){
			String json = file.ReadToEnd();
			var words = JsonConvert.DeserializeObject<String[]>(json);
			return words;
		}
	}


	protected void LogResultsIfChecked(object sender, EventArgs a){
		if (enableLogging.Active) {
			try{
				// try to record the results based off of user preferences
				String testResults = String.Format("{0}|{1}|{2}|{3}", 
					String.Join(",", wordsTwelve), String.Join(",", answersWordsTwelve),
					Convert.ToString(numberCorrectTwelve), DateTime.Now.ToString());
				File.AppendAllText ("output/saved_results.txt", testResults);
				displayLabel.Text = "Results saved to: output/saved_results.txt";
			} catch (Exception e){
				// display an error message if the attempt to save results failed
				displayLabel.Text = "Error: Your results were not saved.  Check to make sure that a file called" +
					" saved_results.txt is in the output folder.";
			}
		}

		okSubmit.Clicked -= LogResultsIfChecked;
		okSubmit.Clicked += OnDeleteEvent;
		
	}

	// starts the twelve words test for the user
	protected void OnBeginTest (object sender, EventArgs a){
		okSubmit.Visible = false;

		displayLabel.Visible = false;

		okSubmit.Clicked -= OnBeginTest;

		StartTimer ();

	}

	protected void OnDeleteEvent(object sender, EventArgs a = null){
		Application.Quit ();
		Environment.Exit (0);
	}

	// shows or hides the input text boxes
	protected void SetVisibleForAllTextBoxes(bool setValue){
		// .Select doesn't work here
		// using a standard foreach(var word in gridWords.Children) also works
		Array.ForEach (gridWords.Children, word => word.Visible = setValue);

	}

	protected void SetTextForAllTextBoxes(String setValue){
		// .Text doesn't work since word is treated as a widget here, and a local cast also fails
		// so using a wrapper method
		// using a standard foreach(var word in gridWords.Children) also works
		Array.ForEach (gridWords.Children, word => SetTextPropertyValue((Gtk.Entry)word, setValue));
	}

	/* I could have made this set to generic properties by using String prop, String val as parameters
	 * and then casting the val based off of the typeof() and .GetProperty and setting it using
	 * .SetValue, but that would basically do an end-run around the type safety of C#, so I deliberately
	 * chose not to do this...  (I'd also have to take an object and a type to typecast, so that it would apply
	 * to more than just a Gtk.Entry object)
	 */
	protected void SetTextPropertyValue(Gtk.Entry word, String setValue){
		word.Text = setValue;
	}


	// starts the timer events (2.5 seconds between them)
	protected void StartTimer(){
		GLib.Timeout.Add (2500, new GLib.TimeoutHandler(UpdateWord));
	}

	// uses timer events to update the words displayed to the user
	protected bool UpdateWord(){
		if (!messageLabel.Visible) {
			messageLabel.Visible = true;
		}

		// at the last word, associate the next function with the ok button and make the ok button and
		// text boxes visible
		// also, tell the timer to stop
		if (currentWordsTwelve >= wordsTwelve.Length) {
			currentWordsTwelve = 0;
			okSubmit.Clicked += EvaluateAnswers;
			messageLabel.Visible = false;
			displayLabel.Text = "Please enter the 12 words that you just saw.";
			displayLabel.Visible = true;
			okSubmit.Visible = true;
			SetVisibleForAllTextBoxes (true);
			return false;
		}
		
		// cycle word so that the current queued word displays and the next word is queued
		messageLabel.Text = wordsTwelve [currentWordsTwelve];
		currentWordsTwelve++;
		return true;
	}

}
