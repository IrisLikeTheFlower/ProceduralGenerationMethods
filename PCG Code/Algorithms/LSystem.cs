using System.Collections.Generic;
using UnityEngine;

public static class LSystem
{
    public class Rule
    {
        public char symbol;
        public string replacement;
        public Rule(char symbol, string replacement)
        {
            this.symbol = symbol;
            this.replacement = replacement;
        }
    }

    public static string GenerateString(string axiom, List<Rule> rules, int iterations)
    {
        string current = axiom;
        for (int i = 0; i < iterations; i++)
        {
            string next = "";
            foreach (char c in current)
            {
                bool replaced = false;
                foreach (var rule in rules)
                {
                    if (rule.symbol == c)
                    {
                        next += rule.replacement;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) next += c;
            }
            current = next;
        }
        return current;
    }

    public struct Segment
    {
        public Vector2 start;
        public Vector2 end;
        public Segment(Vector2 start, Vector2 end) { this.start = start; this.end = end; }
    }

    public static List<Segment> Interpret(string commands,
        float initialStep, float initialAngleStep,
        float stepDecay, float angleDecay)
    {
        List<Segment> segments = new List<Segment>();
        Vector2 currentPos = Vector2.zero;
        float currentStep = initialStep;
        float currentAngleStep = initialAngleStep;
        float currentAngle = 90f; // 0° = вправо, 90° = вгору
        Stack<(Vector2 pos, float step, float angleStep, float angle)> stack = new Stack<(Vector2, float, float, float)>();

        foreach (char c in commands)
        {
            switch (c)
            {
                case 'F':
                    Vector2 dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                    Vector2 newPos = currentPos + dir * currentStep;
                    segments.Add(new Segment(currentPos, newPos));
                    currentPos = newPos;
                    break;
                case 'f':
                    dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                    currentPos += dir * currentStep;
                    break;
                case '+':
                    currentAngle += currentAngleStep;
                    break;
                case '-':
                    currentAngle -= currentAngleStep;
                    break;
                case '[':
                    stack.Push((currentPos, currentStep, currentAngleStep, currentAngle));
                    break;
                case ']':
                    var state = stack.Pop();
                    currentPos = state.pos;
                    currentStep = state.step;
                    currentAngleStep = state.angleStep;
                    currentAngle = state.angle;
                    break;
                case '~':
                    currentStep *= stepDecay;
                    break;
                case '^':
                    currentAngleStep *= angleDecay;
                    break;
                default:
                    break;
            }
        }
        return segments;
    }
}