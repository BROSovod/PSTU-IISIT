namespace ProductionSystem
{
	public class ProductionSystem : IProductionSystem
	{
		private IKnowledgeBase knowledgeBase;
		private IInferenceEngine inferenceEngine;
		IFactsReceiver factsReceiver;
		IWorkingMemory workingMemory;

		public ProductionSystem(string wmPath, IFactsReceiver factsReceiver)
		{
            Rule[] rules = JsonRuleReader.Parse(wmPath).ToArray();

            knowledgeBase = new KnowledgeBase(rules);

            inferenceEngine = new InferenceEngine();

            this.factsReceiver = factsReceiver;

            workingMemory = new WorkingMemory();
		}

        public event FactFixedEventHandler? OnFactFixed;

        public Tuple<string, IEnumerable<FixedFact>?>? Explain(string factName)
        {
            Tuple<FixedFact, Rule?>? tuple = workingMemory.GetFactAndRule(factName);

            if (tuple == null || tuple.Item2 == null)
                return null;

            string ruleName = tuple.Item2.RuleName;

            ExclusiveList<FixedFact>? facts = tuple.Item2.Explain(workingMemory);

            return new Tuple<string, IEnumerable<FixedFact>?>(ruleName, facts);
        }

        public void Run(List<string> importantFacts)
        {
            for(FixedFact? newFact = factsReceiver.GetNewFact(workingMemory);
                newFact != null;
                newFact = factsReceiver.GetNewFact(workingMemory))
                do
                {
                    //��������� ���� � ������� ������
                    workingMemory.AddFact(newFact);

                    //������� ��� ���������� ������ ����� � ������� ������
                    OnFactFixed?.Invoke(newFact);

                    //������������� ���������������� ������
                    ILockEnumerator<Rule> enumerator = knowledgeBase.GetUnlockedEnumerator();

                    //���������� � ��� �������� ������ � ���������� ����������������
                    inferenceEngine.Sort(enumerator, workingMemory);

                    //������� ����� ���� � ������� ���
                    newFact = inferenceEngine.Infer();
                }
                //���� �� ���������� �������� ������� � ���
                while (newFact != null);
        }
    }
}