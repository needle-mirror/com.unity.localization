using NUnit.Framework;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace UnityEngine.Localization.SmartFormat.Tests.Extensions
{
    public class GlobalVariablesSourceTests
    {
        SmartFormatter m_Formatter;
        PersistentVariablesSource m_GlobalVariablesSource;
        VariablesGroupAsset m_Group1;
        VariablesGroupAsset m_Group2;
        VariablesGroupAsset m_NestedGroup1;
        VariablesGroupAsset m_NestedGroup2;

        [System.Serializable]
        class CharacterDetails : IVariable
        {
            public string FirstName { get; set; }
            public string Surname { get; set; }
            public int Age { get; set; }
            public bool Alive { get; set; }

            public object GetSourceValue(ISelectorInfo _) => this;
        }

        struct ReturnValue : IVariable
        {
            public object SourceValue { get; set; }

            public object GetSourceValue(ISelectorInfo _) => SourceValue;
        }

        class CharacterDetailsNoReflection : IVariableGroup, IVariable
        {
            public string Name { get; set; }

            public object GetSourceValue(ISelectorInfo _) => this;

            public bool TryGetValue(string name, out IVariable value)
            {
                switch (name)
                {
                    case "Name":
                        value = new ReturnValue { SourceValue = "Max Caulfield" };
                        return true;
                    case "Age":
                        value = new ReturnValue { SourceValue = 18 };
                        return true;
                    case "Home":
                        value = new ReturnValue { SourceValue = "Arcadia Bay" };
                        return true;
                }
                value = default;
                return false;
            }
        }

        [SetUp]
        public void Setup()
        {
            m_Formatter = Smart.CreateDefaultSmartFormat();
            m_GlobalVariablesSource = new PersistentVariablesSource(m_Formatter);
            m_Formatter.AddExtensions(m_GlobalVariablesSource);

            m_NestedGroup1 = ScriptableObject.CreateInstance<VariablesGroupAsset>();
            m_NestedGroup2 = ScriptableObject.CreateInstance<VariablesGroupAsset>();

            m_Group1 = ScriptableObject.CreateInstance<VariablesGroupAsset>();
            m_GlobalVariablesSource.Add("global", m_Group1);
            m_Group1.Add("myInt", new IntVariable { Value = 123 });
            m_Group1.Add("my-Int-var", new IntVariable());
            m_Group1.Add("apple-count", new IntVariable { Value = 10 });
            m_Group1.Add("myFloat", new FloatVariable { Value = 1.23f });
            m_Group1.Add("some-float-value", new FloatVariable());
            m_Group1.Add("time", new FloatVariable { Value = 1.234f });
            m_Group1.Add("door-state", new BoolVariable { Value = true });
            m_Group1.Add("max", new CharacterDetailsNoReflection());

            m_Group2 = ScriptableObject.CreateInstance<VariablesGroupAsset>();
            m_GlobalVariablesSource.Add("npc", m_Group2);
            m_Group2.Add("emily", new CharacterDetails { FirstName = "Emily", Surname = "Kaldwin", Age = 20, Alive = true });
            m_Group2.Add("guy", new CharacterDetails { FirstName = "Guy", Surname = "Threepwood", Alive = false });

            // Nested groups
            m_Group1.Add("nested", new NestedVariablesGroup { Value = m_NestedGroup1 });
            m_NestedGroup1.Add("further-nested", new NestedVariablesGroup { Value = m_NestedGroup2 });
            m_NestedGroup1.Add("nested-float", new FloatVariable { Value = 0.12345f });
            m_NestedGroup2.Add("my-nested-int", new IntVariable { Value = 1111 });
            m_NestedGroup2.Add("my-nested-string", new StringVariable { Value = "I am nested deep" });
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Group1);
            Object.DestroyImmediate(m_Group2);
            Object.DestroyImmediate(m_NestedGroup1);
            Object.DestroyImmediate(m_NestedGroup2);
        }

        [TestCase("My Int Value is 123", "My Int Value is {global.myInt}")]
        [TestCase("I have 10 apples", "I have {global.apple-count:plural:apple|{} apples}")]
        [TestCase("Hello Emily Kaldwin", "Hello {npc.emily.FirstName} {npc.emily.Surname}")]
        [TestCase("The door is open.", "The door is {global.door-state:open|closed}.")]
        [TestCase("Emily is 20 years old and still alive", "{npc.emily.FirstName} is {npc.emily.Age} years old and {npc.emily.Alive:still alive|dead}")]
        [TestCase("Unlike Emily, Guy Threepwood is dead.", "Unlike {npc.emily.FirstName}, {npc.guy.FirstName} {npc.guy.Surname} is {npc.guy.Alive:still alive|dead}.")]
        [TestCase("Max Caulfield is from Arcadia Bay and is 18 years old.", "{global.max.Name} is from {global.max.Home} and is {global.max.Age} years old.")]
        [TestCase("This value is nested 1 deep: 0.12345", "This value is nested 1 deep: {global.nested.nested-float}")]
        [TestCase("This value is nested 2 deep: 1111 - I am nested deep", "This value is nested 2 deep: {global.nested.further-nested.my-nested-int} - {global.nested.further-nested.my-nested-string}")]
        public void Format_UsesExpectedVariables(string expected, string format)
        {
            var result = m_Formatter.Format(format, null);
            Assert.AreEqual(expected, result);
        }
    }
}
