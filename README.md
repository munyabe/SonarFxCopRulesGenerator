SonarFxCopRulesGenerator
========================

The tool which generates rules configuration xml for give FxCop assemblies

If you want to buid the source file
-----------------------------------
- you need to have FxCop installed
- open the solution file in Visual Studio 2012
- check the project references and correct them if necessary.
  - <b>FxCopCommon.dll</b> and <b>FxCopSdk.dll</b> are located in <i>%fxcop_home%</i>\FxCopCommon.dll

Note:
<i>%fxcop_home%</i> is usually "c:\Program Files (x86)\Microsoft Fxcop 10.0" on 64b OS and "c:\Program Files\Microsoft Fxcop 10.0" on 32b OS

To generate an XML file for standard FxCop rules
------------------------------------------------
- execute
<pre>SonarFxCopRulesGenerator.exe "<i>%fxcop_home%</i>\rules" YourOutputFile.xml</pre>

To generate an XML file for syour custom FxCop rules
----------------------------------------------------
- execute:
<pre>SonarFxCopRulesGenerator.exe "%location_of_your_assemblies%\rules" YourOutputFile.xml</pre>

To generate an XML file for Visual Studio Static Analysis rules
---------------------------------------------------------------
- copy <b>phx.dll</b> and <b>Microsoft.VisualStudio.CodeAnalysis.Phoenix.dll</b> from "<i>%vs_static_analysis_home%</i>\FxCop" to the folder where your SonarFxCopRulesGenerator.exe is located
- execute:
<pre>SonarFxCopRulesGenerator.exe <i>"%vs_statis_analysis_home%</i>\FxCop\rules" YoutOutputFile.xml</pre>

Note:
<i>%vs_static_analysis_home%</i> is usually "c:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\" on 64b OS and "c:\Program Files\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\" on 32b OS
