using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathInterpreter : MonoBehaviour {
	static bool ERROR_SIGN = false; //When this is true, disable all functions.
	//string boss = "ln ( 2.71828 ^ x + root ( x ^ 2 + 4 ^ 2 ) )";

	static List<string> valid_symbols = new List<string>(new string[] {"+", "-", "*", "/", "(", ")", "^", "root", "sin", "cos", "tan", "cot", "sec", "csc", "lg", "ln", "x"});
	static List<string> num_symbols = new List<string>(new string[] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "."});

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	//Test strings:
	/*
	string a = "x * cos ( x )";
	string b = "x * sin ( x )";

	string superboss = "ln(2.71828^x+root(x^2+4^2))";

	string test = "( 1 + 2 / ( 5 + 6 ) * 10 ) ^ 10 / 20";
	string test1 = "( 4 + 8 ) * ( 6 - 5 ) / ( ( 3 - 2 ) * ( 2 + 2 ) )";
	*/


	public static string replace_first_occurrence (string source, string find, string replace)
	{
		int place = source.IndexOf(find);
		string result = source.Remove(place, find.Length).Insert(place, replace);
		return result;
	}

	static string squeeze(string read_str, char chr) {
		var arr = "";
		var has_chr_before = false;
		for (var i = 0; i < read_str.Length; i++) {
			var str = read_str[i];
			if ((str != chr) || (has_chr_before == false)) {
				if (str == chr) {
					has_chr_before = true;
				} else {
					has_chr_before = false;
				}
				arr += (read_str[i]);
			}
		}
		return arr;
	}

	//TODO: try to upgrade this engine by allowing the following inputs:
	//e.g. -(1+2), -30*(sin(2)), sin(-10);
	public static string block_identifier(string str) {
		string stack_str = "";
		string stack_num = "";
		string result = "";
		int dot_counter = 0;

		str = str.Replace(" ", "");

		for (int i = 0; i < str.Length; i++) {
			if (num_symbols.IndexOf(str[i].ToString()) >= 0) {
				stack_num += str[i];
				if (str[i] == '.') {
					dot_counter++;
					if (dot_counter > 1) { //DO NOT ALLOW MORE THAN 1 DOT IN NUMBER!!!
						Debug.LogError("Not a valid number");
						ERROR_SIGN = true;
					}
				}
			} else {
				dot_counter = 0; //Reset dot counter

				stack_str += str[i];
				result = result + " " + stack_num + " ";
				stack_num = "";
			}
			if (valid_symbols.IndexOf(stack_str) >= 0) {
				result = result + " " + stack_str + " ";
				stack_str = "";
			}
			if (stack_str.Length >= 4) { //Normally its max is 4, but when 4 it should have been popped already.
				print(stack_str);
				Debug.LogError("String input error");
				ERROR_SIGN = true;
			}
		}

		result = result + " " + stack_num + " ";
		result = squeeze(result, ' ');
		return result;
	}

	public static string Top(List<string> arr) {
		return arr[arr.Count-1];
	}
	public static string Pop(List<string> arr) {
		string temp = arr [arr.Count - 1];
		arr.RemoveAt(arr.Count - 1);
		return temp;
	}
	public static double Pop(List<double> arr) {
		double temp = arr [arr.Count - 1];
		arr.RemoveAt(arr.Count - 1);
		return temp;
	}

	public static bool is_operator(string str) {
		switch (str) {
			case "+":
			case "-":
			case "*":
			case "/":
			case "^":
			case "(":
			case ")":
				return true;
			default:
				return false;
		}
	}

	static List<string> trig_and_notation = new List<string>(new string[] {"sin", "cos", "tan", "cot", "sec", "csc", "lg", "ln", "root"});

	public static bool is_trig_or_notation(string str) {
		if (trig_and_notation.IndexOf (str) >= 0) {
			return true;
		}
		return false;
	}

	public static bool is_num(string str, string prev = "NaN") {
		int dot_num = 0;
		for (int i = 0; i < str.Length; i++) {
			//Unfortunately, due to unity system limitations, I cannot use anonymous functions here.
			switch(str[i]) {
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				continue;
			//Fix the following case in later updates.
			/*
			case '-':
				if (i != 0 && prev != "(") {
					return false;
				}
				break;
			*/
			case '.':
				dot_num++;
				break;
			default:
				return false;
			}
		}
		if (dot_num <= 1) {
			return true;
		}
		return false;
	}

	//Split the string to arr based on spc, this is adapted from my another project, lineutils.
	public static List<string> split_by_pattern(string str, string line) {
		List<string> arr = new List<string>();
		string temp_str = line;
		int subindex = -1;
		do {
			subindex = temp_str.IndexOf(str);
			if (subindex > -1) {
				arr.Add(temp_str.Substring(0, subindex));
				temp_str = temp_str.Substring(subindex, temp_str.Length-subindex);
				temp_str = replace_first_occurrence(temp_str, str, "");
			}
		} while (subindex > -1);
		arr.Add(temp_str);
		while (arr.IndexOf("") > -1){
			arr.Remove("");
		} //remove extra "" entry
		return arr;
	}

	public static List<string> infix_to_postfix(string str) {
		if (ERROR_SIGN == true) {
			Debug.LogError("ERROR_SIGN ON");
			return new List<string>();
		}
		str = block_identifier(str);
		return infix_to_postfix_raw(str);
	}

	//Notice about this input: I deployed the "[" square brackets in place of "(" to help distinguish function usage and
	//normal bracket blocks. However, this will, unfortunately, introduce identification-related complications when 
	//we meet with some normal mathematical expressions
	static List<string> infix_to_postfix_raw(string str) {
		List<string> infix = split_by_pattern (" ", str);
		List<string> postfix = new List<string>();
		List<string> op_stack = new List<string>();
		List<string> notation_stack = new List<string>();
		//Record previous so that could change to '[' when prev is notation
		var prev = " ";

		foreach (string curr in infix) {
			if (curr == "x" || is_num(curr)) { //Normal operands
				postfix.Add(curr);
			} else if (is_operator(curr)) { //Operator
				if (curr == ")") { //If )
					while (Top(op_stack) != "(" && Top(op_stack) != "[") {
						postfix.Add(Pop(op_stack)); //pop to postfix till met with brackets
					}
					string bracket = Pop(op_stack); //pop the bracket for check
					if (bracket == "[") { //if [, should pop the last notation to stack
						string a = Pop(notation_stack);
						postfix.Add(a);
					}
				} else if (op_stack.Count != 0) { //Other operator
					while (op_stack.Count > 0 && (get_precedence(Top(op_stack)) >= get_precedence(curr)) && Top(op_stack) != "(" && Top(op_stack) != "[") { //pop all lower/equal precedence operators
						postfix.Add(Pop(op_stack));
					}
					if (curr == "(" && is_trig_or_notation(prev)) { //special handle [ for notation prev
						op_stack.Add("[");
					} else {
						op_stack.Add(curr);
					}
				} else {
					if (curr == "(" && is_trig_or_notation(prev)) { //special handle [ for notation prev
						op_stack.Add("[");
					} else {
						op_stack.Add(curr);
					}
				}
			} else if (is_trig_or_notation(curr)) {
				notation_stack.Add(curr);
			}
			prev = curr; //update prev
		};

		while (op_stack.Count != 0) {
			string temp = Pop (op_stack);
			if (temp != "(" && temp != "[")
				postfix.Add(temp);
		}
		while (notation_stack.Count != 0) {
			postfix.Add(Pop(notation_stack));
		}
		return postfix;
	}

	static int get_precedence(string str) {
		switch (str) {
		case "+":
		case "-":
			return 100;
		case "*":
		case "/":
			return 200;
		case "^":
			return 300;
		case "(":
		case ")":
		case "[":
			return 400;
		default:
			return -1;
		}
	}
}
