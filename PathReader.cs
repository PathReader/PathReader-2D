using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PathReader : MonoBehaviour {

	//public PathInterpreter pathInterpreter;
	private float ERROR_NUM = Int16.MaxValue;
	private bool isError = false;

	public string XCoordinate;
	public string YCoordinate;

	//WARNING: THIS SPEED IS NOT STANDARDIZED!!!
	public double startingParameter = 0;
	private double param;
	public double speed = 1;

	public bool useBound = false;
	public double upperBound = 0;
	public double lowerBound = 0;

	private double trueSpeed;


	private List<string> x_param, y_param;


	//Tests:
	/*
	string boss = "ln ( 2.71828 ^ x + root ( x ^ 2 + 4 ^ 2 ) )";
	string test1 = "x^2/4";
	string test2 = "sin(3.1415926/6)";
	string test3 = "ln(100)";
	string test4 = "root(123)";
	string circleX = "cos(x)";
	string circleY = "sin(x)";
	double test_num = 0f;
	double test_pace = 0.05f;
	List<string> circle_x;
	List<string> circle_y;
	//string beautiful1X = "2.5*(sin((0-5)*x))^2 * 2 ^ (cos(cos(4.28-2.3*x)))";
	//string beautiful1X = "(cos(2 * 3.14159265359 * x))^3 + cos(2*3.1415926* 50 *x) -1";
	string beautiful1X = "sin(x) * (2.71828 ^ (cos(x)) - 2 * cos(4 * x) - (sin(x / 12))^5) ";
	string beautiful1Y = "cos(x) * (2.71828 ^ (cos(x)) - 2 * cos(4 * x) - (sin(x / 12))^5) ";

	//string beautiful1Y = "2.5*sin(sin((0-5)*x)) * (cos(4.28 * 2.3 * x)) ^ 2";
	//string beautiful1Y = "(sin(2 * 3.14159265359 * x))^3 + sin(2*3.1415926 * 50 * x)";
	List<string> beautiful1_x;
	List<string> beautiful1_y;
	*/

	void Start () {
		x_param = PathInterpreter.infix_to_postfix (XCoordinate);
		y_param = PathInterpreter.infix_to_postfix (YCoordinate);
		param = startingParameter;

		trueSpeed = speed / 100;
		//Tests:
		string examine_x = "";
		foreach (string a in x_param) {
			examine_x = examine_x + a + " ";
		}
		print (examine_x);

		string examine_y = "";
		foreach (string b in y_param) {
			examine_y = examine_y + b + " ";
		}
		print (examine_y);
	}
	
	// Update is called once per frame
	void Update () {
		param += trueSpeed;
		if (useBound) {
			if (param >= upperBound) {
				param = lowerBound;
			} else if (param <= lowerBound) {
				param = upperBound;
			}
		}

		double xcoord = get_coord (param, x_param);
		double ycoord = get_coord (param, y_param);

		transform.position = new Vector2 ((float)xcoord, (float)ycoord);
	}

	double get_coord(double x, List<string> param) {
		
		List<double> vals = new List<double> ();

		double value = 0;
		double second_value = 0;
		bool is_value_active = false;
		bool is_second_active = false;

		foreach (string a in param) {
			if (PathInterpreter.is_num (a) || a == "x") {
				if (a == "x") {
					vals.Add (x);
				} else {
					print (a + "parseerror");
					vals.Add (double.Parse(a));
				}
			} else if (PathInterpreter.is_operator (a)) {
				switch (a) {
					case "+":
						try {
							vals [vals.Count - 2] += vals [vals.Count - 1];
							//value += second_value;
							PathInterpreter.Pop(vals);
						} catch (Exception e) {
							return ERROR_NUM;
						}
						break;
					case "-":
						try {
							vals [vals.Count - 2] -= vals [vals.Count - 1];
							//value -= second_value;
							PathInterpreter.Pop(vals);
						} catch (Exception e) {
							return ERROR_NUM;
						}
						break;
					case "*":
						try {
							vals [vals.Count - 2] *= vals [vals.Count - 1];
							//value *= second_value;
							PathInterpreter.Pop(vals);
						} catch (Exception e) {
							return ERROR_NUM;
						}
						break;
					case "/":
						try {
							vals [vals.Count - 2] /= vals [vals.Count - 1];
							//value /= second_value;
							PathInterpreter.Pop(vals);
						} catch (Exception e) {
							return ERROR_NUM;
						}
						break;
					case "^":
						try {
							vals[vals.Count-2] = Math.Pow (vals[vals.Count-2], vals[vals.Count-1]);
							PathInterpreter.Pop(vals);
							//Debug.LogWarning(value + " " + second_value);
							//value = Mathf.Pow (value, second_value);
						} catch (Exception e) {
							return ERROR_NUM;
						}
						break;
				default:
					print (a);
						Debug.LogError ("Invalid Operator Format!");
						return ERROR_NUM;
				}
			} else if (PathInterpreter.is_trig_or_notation (a)) {
				switch (a) {
				case "sin":
					vals [vals.Count - 1] = Math.Sin (vals [vals.Count - 1]);
					//operand = Mathf.Sin (operand);
					break;
				case "cos":
					vals [vals.Count - 1] = Math.Cos (vals [vals.Count - 1]);
					//operand = Mathf.Cos (operand);
					break;
				case "tan":
					try {
						vals [vals.Count - 1] = Math.Tan (vals [vals.Count - 1]);
						//operand = Mathf.Tan (operand);
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				case "cot":
					try {
						vals [vals.Count - 1] = 1 / (Math.Tan (vals [vals.Count - 1]));
						//operand = 1 / (Mathf.Tan (operand));
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				case "sec":
					try {
						vals [vals.Count - 1] = 1 / (Math.Cos (vals [vals.Count - 1]));
						//operand = 1 / (Mathf.Cos (operand));
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				case "csc":
					try {
						vals [vals.Count - 1] = 1 / (Math.Sin (vals [vals.Count - 1]));
						//operand = 1 / (Mathf.Sin (operand));
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				case "ln":
					try {
						vals [vals.Count - 1] = Math.Log (vals [vals.Count - 1]);
						//operand = Mathf.Log (operand);
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				case "lg":
					try {
						vals [vals.Count - 1] = Math.Log10 (vals [vals.Count - 1]);
						//operand = Mathf.Log10 (operand);
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				case "root":
					try {
						vals [vals.Count - 1] = Math.Sqrt (vals [vals.Count - 1]);
						//print("ha?" + operand);
						//operand = Mathf.Sqrt(operand);
					} catch (Exception e) {
						return ERROR_NUM;
					}
					break;
				default:
					return ERROR_NUM;
					break;
				}
			}
		}
		return vals [0];
		//return value;
	}
}
