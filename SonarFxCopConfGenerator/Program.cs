using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using Microsoft.FxCop.Common;
using Microsoft.FxCop.Sdk;
using Microsoft.VisualStudio.CodeAnalysis.Extensibility;


namespace SonarFxCopConfGenerator
{
    class Program
    {
        private const string LINE_BREAK = "<br/>";
        private const string EMPTY_LINE = LINE_BREAK + LINE_BREAK;
        private const string A_HREF_FORMAT = "<a href=\"{0}\">{1}</a>";
        private const string MSDN_LINK_CONTENT = "MSDN help page of rule {0}";
        private const string MSDN_LINK = "http://msdn.microsoft.com/library/";
        private static ArgumentsParser myArgumentsParser;

        static void Main(string[] args)
        {
            myArgumentsParser = new ArgumentsParser(args);
            if (!myArgumentsParser.AreArgumentsValid)
            {
                return;
            }

            IEnumerable<string> assemblyNames = myArgumentsParser.SelectedAssemblies;
            string outputFile = myArgumentsParser.OutputPath;

            IEnumerable<XElement> ruleElements = GetRuleElementsFromAssemblies(assemblyNames).ToList();

            var outputSonarFxCopConfiguration = new XDocument(new XElement("rules", ruleElements));
            Console.WriteLine("Writing {0} FxCop rules to {1} file.", outputSonarFxCopConfiguration.Descendants("rule").Count(), outputFile);
            outputSonarFxCopConfiguration.Save(outputFile);
        }

        private static IEnumerable<XElement> GetRuleElementsFromAssemblies(IEnumerable<string> assemblyNames)
        {
            return assemblyNames.SelectMany(GetRuleElementsFromAssembly);
        }

        private static IEnumerable<XElement> GetRuleElementsFromAssembly(string assemblyName)
        {
            List<IRuleInfo> customFxCopRules = GetCustomFxCopRules(assemblyName).ToList();
            if (!customFxCopRules.Any())
            {
                customFxCopRules = GetPhoenixFxCopRules(assemblyName).ToList();
            }
            if (!customFxCopRules.Any())
            {
                Console.WriteLine("No rules found in the provided assembly {0}.", assemblyName);
            }
            return from ruleInfo in customFxCopRules
                   select
                           new XElement("rule",
                                        new XAttribute("key", ruleInfo.Name),
                                        new XElement("name", GetHumanReadableName(ruleInfo)),
                                        new XElement("configKey",
                                                     new XCData(string.Format("{0}@$(FxCopDir)\\Rules\\{1}.dll",
                                                                              ruleInfo.Name,
                                                                              ruleInfo.Namespace))),
                                        new XElement("category",
                                                     new XAttribute("name", ruleInfo.Category.Id)),
                                        new XElement("description",
                                                     new XCData(GetDescriptionText(ruleInfo))));
        }

        private static string GetDescriptionText(IRuleInfo ruleInfo)
        {
            var stringBuilder = new StringBuilder(ruleInfo.Description);
            if (ruleInfo.HelpUrl != null)
            {
                stringBuilder.AppendLine(LINE_BREAK);
                stringBuilder.AppendLine(GetLinkFormat(ruleInfo));
            }
            else
            {
                stringBuilder.AppendLine(EMPTY_LINE);
                stringBuilder.AppendFormat("For reference, rule ID is {0}", ruleInfo.Id);
            }
            return stringBuilder.ToString();
        }

        private static string GetLinkFormat(IRuleInfo ruleInfo)
        {
            string url;
            if (ruleInfo is CustomRuleInfo)
            {
                url = ((CustomRuleInfo) ruleInfo).HelpUrlAsString;
                url = url ?? string.Empty;
            }
            else
            {
                url = ruleInfo.HelpUrl == null ? string.Empty : ruleInfo.HelpUrl.ToString();
            }
            string linkContent = url;
            if (myArgumentsParser.IsMicrosoft)
            {
                linkContent = string.Format(MSDN_LINK_CONTENT, ruleInfo.Id);
            }
            return string.Format(A_HREF_FORMAT, url, linkContent);
        }

        private static string GetHumanReadableName(IRuleInfo ruleInfo)
        {
            return ruleInfo is CustomRuleInfo ? ((CustomRuleInfo) ruleInfo).HumanReadableName : ruleInfo.Name;
        }

        private static IEnumerable<IRuleInfo> GetCustomFxCopRules(string assemblyName)
        {
            Assembly customFxCopRulesAssembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyName));
            foreach (Type theEndTypeInAssembly in customFxCopRulesAssembly.GetTypes())
            {
                if (!theEndTypeInAssembly.IsAbstract && theEndTypeInAssembly.GetInterfaces().Contains(typeof (IRule)))
                {
                    yield return new CustomRuleInfo(Activator.CreateInstance(theEndTypeInAssembly) as IRule,
                                                    theEndTypeInAssembly.Name);
                }
            }
        }

        private static IEnumerable<IRuleInfo> GetPhoenixFxCopRules(string assemblyName)
        {
            var discovery = new Discovery();
            discovery.LoadAssemblyFrom(assemblyName);
            return discovery.GetExtensions<RuleInfo>();
        }

        private class CustomRuleInfo : IRuleInfo
        {
            public CustomRuleInfo(IRule fxcopRule, string typeName)
            {
                FullId = fxcopRule.CheckId;
                Namespace = fxcopRule.GetType().Assembly.GetName().Name;
                Id = fxcopRule.CheckId;
                Category = new FxCopCategoryInfo(fxcopRule.Category);
                Name = typeName;
                HumanReadableName = fxcopRule.Name;
                Description = fxcopRule.Description;
                AnalyzerId = null;
                HelpKeyword = null;
                Console.WriteLine(fxcopRule.Url);
                try
                {
                    if (!string.IsNullOrWhiteSpace(fxcopRule.Url))
                    {
                        var link = fxcopRule.Url;
                        if (myArgumentsParser.IsMicrosoft)
                        {
                            link = link.Replace("@", MSDN_LINK);
                        }
                        HelpUrl = new Uri(link);
                        HelpUrlAsString = HelpUrl.ToString();
                    }
                }
                catch (UriFormatException)
                {
                    HelpUrlAsString = fxcopRule.Url;
                }
                FixCategories = FixCategories.None;
            }

            public string HumanReadableName { get; private set; }

            public string HelpUrlAsString { get; private set; }

            public string FullId { get; private set; }

            public string Namespace { get; private set; }

            public string Id { get; private set; }

            public string AnalyzerId { get; private set; }

            public ICategoryInfo Category { get; private set; }

            public string Name { get; private set; }

            public string Description { get; private set; }

            public Uri HelpUrl { get; private set; }

            public string HelpKeyword { get; private set; }

            public FixCategories FixCategories { get; private set; }
        }
    }
}
