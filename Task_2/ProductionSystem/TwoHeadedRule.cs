﻿namespace ProductionSystem
{
    internal class TwoHeadedRule : Rule
    {
        public readonly string ReverseValue;

        public TwoHeadedRule(IRulePart rootRulePart, string name, string directValue, string reverseValue, int importancy) : base(rootRulePart, name, directValue, importancy)
        {
            ReverseValue = reverseValue;
        }

        public override RuleState GetRuleState(IFactsProvider factsProvider)
        {
            switch (_rootRulePart.GetState(factsProvider))
            {
                case null:
                    return RuleState.Valid;
                case true:
                    return RuleState.Active;
                case false:
                    return RuleState.Invalid;
            }
        }
    }
}
