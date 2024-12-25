using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LSystem<T>
{
    [TextArea(2, 10)]
    public string Rules;

    private Dictionary<char, string> rules;

    public Dictionary<char, ILVariable> Variables { get; private set; }
    public Dictionary<char, LConstant<T>> Constants { get; private set; }

    public string chain;

    private Func<LContext> initialContext;

    private List<ILVariable> children;

    public void Initialize(IEnumerable<ILVariable> variables, IEnumerable<LConstant<T>> constants, Func<LContext> initialContext)
    {
        GenerateRulesDictionary();
        GenerateVariableDictionary(variables);
        GenerateConstantDictionary(constants);

        children = new();

        this.initialContext = initialContext;
        chain = Iterate();
    }

    private void GenerateRulesDictionary()
    {
        rules = new();

        var statements = Rules.Trim().Replace(" ", "").Split(';').ToList();
        statements.RemoveAll(x => string.IsNullOrEmpty(x));

        foreach (var statement in statements)
        {
            var sides = statement.Split(':');

            rules[sides[0].First()] = sides[1];
        }
    }

    private void GenerateVariableDictionary(IEnumerable<ILVariable> variablesToAdd)
    {
        Variables = new();

        foreach (var variable in variablesToAdd)
        {
            Variables[variable.Key] = variable;
        }
    }

    private void GenerateConstantDictionary(IEnumerable<LConstant<T>> constantsToAdd)
    {
        Constants = new();

        foreach (var constant in constantsToAdd)
        {
            Constants[constant.Key] = constant;
        }
    }

    public string Iterate(int iterations = 1)
    {
        foreach (var child in children)
        {
            child.Dispose();
        }

        children.Clear();

        for (int i = 0; i < iterations; i++)
        {
            string result = "";

            foreach (var letter in chain)
            {
                if (rules.TryGetValue(letter, out var rule))
                {
                    result += rule;
                }
                else
                {
                    result += letter;
                }
            }

            chain = result;
        }

        Stack<LContext> contextStack = new();
        contextStack.Push(initialContext.Invoke());
        var currentContext = contextStack.Pop();

        for (int i = 0; i < chain.Length; i++)
        {
            if (chain[i] == '[')
            {
                contextStack.Push(currentContext);
            }
            else if (chain[i] == ']')
            {
                currentContext = contextStack.Pop();
            }
            else if (Constants.TryGetValue(chain[i], out var constant))
            {
                currentContext = constant.Apply(currentContext);
            }
            else if (Variables.TryGetValue(chain[i], out var variable))
            {
                var clone = variable.Clone(currentContext);
                children.Add(clone);
            }
        }

        foreach (var variable in Variables)
        {
            variable.Value.Generated();
        }

        return chain;
    }
}