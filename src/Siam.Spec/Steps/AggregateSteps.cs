using System;
using TechTalk.SpecFlow;

namespace Siam.Spec.Steps
{
    [Binding]
    [Scope(Feature = "Проверка агрегата Памятка", Scenario = "Обновление памятки")]
    public class AggregateSteps
    {
        [Given(@"существует агрегат \""(.*)\""")]
        [Given(@"существует агрегат \""(.*)\"" с параметрами")]
        public void ДопустимАгрегатПамятка(DateTime value)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"взывали команду \[Обновить памятку]")]
        public void ЕслиВзывалиКомандуОбновитьПамятку()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"агрегат выбрасывает событие \[Памятка обновлена]")]
        public void ТоАгрегатВыбрасываетСобытиеПамяткаОбновлена()
        {
            ScenarioContext.Current.Pending();
        }


    }
}
