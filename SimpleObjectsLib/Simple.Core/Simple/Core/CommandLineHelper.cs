using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Simple
{
    /// <summary>Implements a basic command-line switch by taking the
    /// switching name and the associated description.</summary>
    /// <remark>Only currently is implemented for properties, so all
    /// auto-switching variables should have a get/set method supplied.</remark>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineSwitchAttribute : System.Attribute
    {
        /// <summary>Attribute constructor.</summary>
        public CommandLineSwitchAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        
        /// <summary>Accessor for retrieving the switch-name for an associated
        /// property.</summary>
        public string Name { get; private set; }

        /// <summary>Accessor for retrieving the description for a switch of
        /// an associated property.</summary>
        public string Description { get; private set; }
    }

    /// <summary>
    /// This class implements an alias attribute to work in conjunction
    /// with the <see cref="CommandLineSwitchAttribute">CommandLineSwitchAttribute</see>
    /// attribute.  If the CommandLineSwitchAttribute exists, then this attribute
    /// defines an alias for it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineAliasAttribute : System.Attribute
    {
        public CommandLineAliasAttribute(string alias)
        {
            this.Alias = alias;
        }

        public string Alias { get; set; }
    }

    /// <summary>Implementation of a command-line parsing class.  Is capable of
    /// having switches registered with it directly or can examine a registered
    /// class for any properties with the appropriate attributes appended to
    /// them.</summary>
    public class CommandLineParser
    {
        #region Private Members
        
        private string commandLine = String.Empty;
        private string workingString = String.Empty;
        private string applicationName = String.Empty;
        private string[] splitParameters = null;
        private List<SwitchRecord> switches = null;
        
        #endregion

        #region Constructors
        
        public CommandLineParser(string commandLine)
        {
            this.commandLine = commandLine;
        }

        public CommandLineParser(string commandLine, object classForAutoAttributes)
        {
            this.commandLine = commandLine;

            Type type = classForAutoAttributes.GetType();
            System.Reflection.MemberInfo[] members = type.GetMembers();

            for (int i = 0; i < members.Length; i++)
            {
                object[] attributes = members[i].GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    SwitchRecord switchRecord = null;

                    foreach (Attribute attribute in attributes)
                    {
                        if (attribute is CommandLineSwitchAttribute)
                        {
                            CommandLineSwitchAttribute switchAttrib =
								(CommandLineSwitchAttribute)attribute;

                            // Get the property information.  We're only handling
                            // properties at the moment!
                            if (members[i] is System.Reflection.PropertyInfo)
                            {
                                System.Reflection.PropertyInfo propertyInfo = (System.Reflection.PropertyInfo)members[i];

                                switchRecord = new SwitchRecord(switchAttrib.Name, switchAttrib.Description, propertyInfo.PropertyType);

                                // Map in the Get/Set methods.
                                switchRecord.SetMethod = propertyInfo.GetSetMethod();
                                switchRecord.GetMethod = propertyInfo.GetGetMethod();
                                switchRecord.PropertyOwner = classForAutoAttributes;

                                // Can only handle a single switch for each property
                                // (otherwise the parsing of aliases gets silly...)
                                break;
                            }
                        }
                    }

                    // See if any aliases are required.  We can only do this after
                    // a switch has been registered and the framework doesn't make
                    // any guarantees about the order of attributes, so we have to
                    // walk the collection a second time.
                    if (switchRecord != null)
                    {
                        foreach (Attribute attribute in attributes)
                        {
                            if (attribute is CommandLineAliasAttribute)
                            {
                                CommandLineAliasAttribute aliasAttrib = (CommandLineAliasAttribute)attribute;
                                switchRecord.AddAlias(aliasAttrib.Alias);
                            }
                        }
                    }

                    // Assuming we have a switch record (that may or may not have
                    // aliases), add it to the collection of switches.
                    if (switchRecord != null)
                    {
                        if (this.switches == null)
                        {
                            this.switches = new List<SwitchRecord>();
                        }

                        this.switches.Add(switchRecord);
                    }
                }
            }
        }
        
        #endregion

        #region Private Utility Functions
        private void ExtractApplicationName()
        {
            Regex regex = new Regex(@"^(?<commandLine>("".+""|(\S)+))(?<remainder>.+)",
                RegexOptions.ExplicitCapture);
            Match match = regex.Match(commandLine);

            if (match != null && match.Groups["commandLine"] != null)
            {
                this.applicationName = match.Groups["commandLine"].Value;
                this.workingString = match.Groups["remainder"].Value;
            }
        }

        private void SplitParameters()
        {
            // Populate the split parameters array with the remaining parameters.
            // Note that if quotes are used, the quotes are removed.
            // e.g.   one two three "four five six"
            //						0 - one
            //						1 - two
            //						2 - three
            //						3 - four five six
            // (e.g. 3 is not in quotes).
            Regex regex = new Regex(@"((\s*(""(?<param>.+?)""|(?<param>\S+))))", RegexOptions.ExplicitCapture);
            MatchCollection matchCollection = regex.Matches(workingString);

            if (matchCollection != null)
            {
                splitParameters = new string[matchCollection.Count];
                
                for (int i = 0; i < matchCollection.Count; i++)
                    splitParameters[i] = matchCollection[i].Groups["param"].Value;
            }
        }

        private void HandleSwitches()
        {
            if (this.switches != null)
            {
                foreach (SwitchRecord switchRecord in this.switches)
                {
                    Regex regex = new Regex(switchRecord.Pattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                    MatchCollection matchCollection = regex.Matches(workingString);

                    if (matchCollection != null)
                    {
                        for (int i=0; i < matchCollection.Count; i++)
                        {
                            string value = null;
                            if (matchCollection[i].Groups != null && matchCollection[i].Groups["value"] != null)
                                value = matchCollection[i].Groups["value"].Value;

                            if (switchRecord.Type == typeof(bool))
                            {
                                bool state = true;
                                // The value string may indicate what value we want.
                                if (matchCollection[i].Groups != null && matchCollection[i].Groups["value"] != null)
                                {
                                    switch (value)
                                    {
                                        case "+":
                                            state = true;
                                            break;
                                        case "-":
                                            state = false;
                                            break;
                                        case "":
                                            if (switchRecord.ReadValue != null)
                                                state = !(bool)switchRecord.ReadValue;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                switchRecord.Notify(state);
                                break;
                            }
                            else if (switchRecord.Type == typeof(string))
                                switchRecord.Notify(value);
                            else if (switchRecord.Type == typeof(int))
                                switchRecord.Notify(int.Parse(value));
                            else if (switchRecord.Type.IsEnum)
                                switchRecord.Notify(System.Enum.Parse(switchRecord.Type, value, true));
                        }
                    }

                    workingString = regex.Replace(workingString, " ");
                }
            }
        }

        #endregion

        #region Public Properties
        
        public object this[string name]
        {
            get
            {
                if (this.switches != null)
                {
                    for (int i = 0; i < switches.Count; i++)
                    {
                        if (string.Compare((switches[i] as SwitchRecord).Name, name, true) == 0)
                        {
                            return (this.switches[i] as SwitchRecord).Value;
                        }
                    }
                }

                return null;
            }
        }

        public string ApplicationName
        {
            get { return applicationName; }
        }

        public string[] Parameters
        {
            get { return splitParameters; }
        }

        public SwitchInfo[] Switches
        {
            get
            {
                if (this.switches == null)
                {
                    return null;
                }
                else
                {
                    SwitchInfo[] switchInfo = new SwitchInfo[this.switches.Count];

                    for (int i = 0; i < this.switches.Count; i++)
                    {
                        switchInfo[i] = new SwitchInfo(switches[i]);
                    }

                    return switchInfo;
                }
            }
        }

        /// <summary>This function returns a list of the unhandled switches
        /// that the parser has seen, but not processed.</summary>
        /// <remark>The unhandled switches are not removed from the remainder
        /// of the command-line.</remark>
        public string[] UnhandledSwitches
        {
            get
            {
                string switchPattern = @"(\s|^)(?<match>(-{1,2}|/)(.+?))(?=(\s|$))";
                Regex regex = new Regex(switchPattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                MatchCollection matchCollection = regex.Matches(workingString);

                if (matchCollection != null)
                {
                    string[] unhandled = new string[matchCollection.Count];
                    for (int i=0; i < matchCollection.Count; i++)
                    {
                        unhandled[i] = matchCollection[i].Groups["match"].Value;
                    }

                    return unhandled;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region Public Methods
        
        public void AddSwitch(string name, string description)
        {
            if (this.switches == null)
            {
                this.switches = new List<SwitchRecord>();
            }

            SwitchRecord switchRecord = new SwitchRecord(name, description);
            this.switches.Add(switchRecord);
        }

        public void AddSwitch(string[] names, string description)
        {
            if (this.switches == null)
            {
                this.switches = new List<SwitchRecord>();
            }
            SwitchRecord switchRecord = new SwitchRecord(names[0], description);

            for (int i = 1; i < names.Length; i++)
            {
                switchRecord.AddAlias(names[i]);
            }
            
            this.switches.Add(switchRecord);
        }

        public bool Parse()
        {
            this.ExtractApplicationName();

            // Remove switches and associated info.
            this.HandleSwitches();

            // Split parameters.
            this.SplitParameters();

            return true;
        }

        public object InternalValue(string name)
        {
            if (this.switches != null)
            {
                for (int i = 0; i < switches.Count; i++)
                {
                    if (string.Compare(this.switches[i].Name, name, true) == 0)
                    {
                        return this.switches[i].InternalValue;
                    }
                }
            }

            return null;
        }
        
        #endregion

        #region Helper Classes

        /// <summary>A simple internal class for passing back to the caller
        /// some information about the switch.  The internals/implementation
        /// of this class has privillaged access to the contents of the
        /// SwitchRecord class.</summary>
        public class SwitchInfo
        {
            private SwitchRecord switchRecord = null;

            /// <summary>
            /// Constructor for the SwitchInfo class.  Note, in order to hide to the outside world
            /// information not necessary to know, the constructor takes a System.Object (aka
            /// object) as it's registering type.  If the type isn't of the correct type, an exception
            /// is thrown.
            /// </summary>
            /// <param name="switchRecord">The SwitchRecord for which this class store information.</param>
            /// <exception cref="ArgumentException">Thrown if the rec parameter is not of
            /// the type SwitchRecord.</exception>
            public SwitchInfo(object switchRecord)
            {
                if (switchRecord is SwitchRecord)
                {
                    this.switchRecord = switchRecord as SwitchRecord;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            public string Name 
            { 
                get { return this.switchRecord.Name; } 
            }
            
            public string Description 
            { 
                get { return this.switchRecord.Description; } 
            }
            
            public string[] Aliases 
            { 
                get { return this.switchRecord.Aliases; } 
            }
            
            public System.Type Type 
            { 
                get { return this.switchRecord.Type; } 
            }
            
            public object Value 
            { 
                get { return this.switchRecord.Value; } 
            }
            
            public object InternalValue 
            { 
                get { return this.switchRecord.InternalValue; } 
            }
            
            public bool IsEnum 
            { 
                get { return this.switchRecord.Type.IsEnum; } 
            }
            
            public string[] Enumerations 
            { 
                get { return this.switchRecord.Enumerations; } 
            }
        }

        /// <summary>
        /// The SwitchRecord is stored within the parser's collection of registered
        /// switches.  This class is private to the outside world.
        /// </summary>
        private class SwitchRecord
        {
            private string name = String.Empty;
            private string description = String.Empty;
            private object value = null;
            private System.Type switchType = typeof(bool);
            private System.Collections.ArrayList aliases = null;
            private string pattern = String.Empty;

            // The following advanced functions allow for callbacks to be
            // made to manipulate the associated data type.
            private System.Reflection.MethodInfo setMethod = null;
            private System.Reflection.MethodInfo getMethod = null;
            private object m_PropertyOwner = null;

            public SwitchRecord(string name, string description)
            {
                this.Initialize(name, description);
            }

            public SwitchRecord(string name, string description, System.Type switchType)
            {
                if (switchType == typeof(bool) || switchType == typeof(string) || switchType == typeof(int) || switchType.IsEnum)
                {
                    this.switchType = switchType;
                    this.Initialize(name, description);
                }
                else
                {
                    throw new ArgumentException("Currently only Ints, Bool and Strings are supported");
                }
            }

            public object Value
            {
                get
                {
                    if (ReadValue != null)
                    {
                        return ReadValue;
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            public object InternalValue
            {
                get { return value; }
            }

            public string Name
            {
                get { return this.name; }
                set { this.name = value; }
            }

            public string Description
            {
                get { return this.description; }
                set { this.description = value; }
            }

            public System.Type Type
            {
                get { return this.switchType; }
            }

            public string[] Aliases
            {
                get 
                { 
                    return (this.aliases != null) ? (string[])this.aliases.ToArray(typeof(string)) : null; 
                }
            }

            public string Pattern
            {
                get { return this.pattern; }
            }

            public System.Reflection.MethodInfo SetMethod
            {
                set { this.setMethod = value; }
            }

            public System.Reflection.MethodInfo GetMethod
            {
                set { this.getMethod = value; }
            }

            public object PropertyOwner
            {
                set { this.m_PropertyOwner = value; }
            }

            public object ReadValue
            {
                get
                {
                    object readValue = null;

                    if (m_PropertyOwner != null && getMethod != null)
                    {
                        readValue = getMethod.Invoke(m_PropertyOwner, null);
                    }

                    return readValue;
                }
            }

            public string[] Enumerations
            {
                get
                {
                    if (this.switchType.IsEnum)
                        return System.Enum.GetNames(this.switchType);
                    else
                        return null;
                }
            }


            public void AddAlias(string alias)
            {
                if (this.aliases == null)
                    this.aliases = new System.Collections.ArrayList();
                this.aliases.Add(alias);

                this.BuildPattern();
            }

            public void Notify(object value)
            {
                if (this.m_PropertyOwner != null && this.setMethod != null)
                {
                    object[] parameters = new object[1];
                    parameters[0] = value;
                    this.setMethod.Invoke(m_PropertyOwner, parameters);
                }
                this.value = value;
            }

            private void Initialize(string name, string description)
            {
                this.name = name;
                this.description = description;

                this.BuildPattern();
            }

            private void BuildPattern()
            {
                string matchString = this.Name;
                string strPatternStart = @"(\s|^)(?<match>(-{1,2}|/)(";
                string strPatternEnd;  // To be defined below.

                if (this.Aliases != null && Aliases.Length > 0)
                {
                    foreach (string s in Aliases)
                    {
                        matchString += "|" + s;
                    }
                }

                // The common suffix ensures that the switches are followed by
                // a white-space OR the end of the string.  This will stop
                // switches such as /help matching /helpme
                //
                string strCommonSuffix = @"(?=(\s|$))";

                if (Type == typeof(bool))
                {
                    strPatternEnd = @")(?<value>(\+|-){0,1}))";
                }
                else if (Type == typeof(string))
                {
                    strPatternEnd = @")(?::|\s+))((?:"")(?<value>.+)(?:"")|(?<value>\S+))";
                }
                else if (Type == typeof(int))
                {
                    strPatternEnd = @")(?::|\s+))((?<value>(-|\+)[0-9]+)|(?<value>[0-9]+))";
                }
                else if (Type.IsEnum)
                {
                    string[] enumNames = Enumerations;
                    string enumName = enumNames[0];

                    for (int i = 1; i < enumNames.Length; i++)
                    {
                        enumName += "|" + enumNames[i];
                    }

                    strPatternEnd = @")(?::|\s+))(?<value>" + enumName + @")";
                }
                else
                {
                    throw new System.ArgumentException();
                }

                // Set the internal regular expression pattern.
                pattern = strPatternStart + matchString + strPatternEnd + strCommonSuffix;
            }
        }

        #endregion
    }
}

///// Usage Example

///// <summary>The application call acts as a tester for the command line
///// parser.  It demonstrates using switch attributes on properties
///// meaning that the coder does not have to implement anything except
///// instantiating the parser in the most basic way.</summary>
//public class Application
//{
//    #region Enumerations
//    public enum DaysOfWeek
//    {
//        Sun,
//        Mon,
//        Tue,
//        Wed,
//        Thu,
//        Fri,
//        Sat
//    };
//    #endregion

//    #region Private Variables
//    private bool m_showHelp = true;
//    private bool m_SomethingElse = false;
//    private string m_SomeName = "My Name";
//    private int m_Age = 999;
//    private string m_test = "XXXX";
//    private DaysOfWeek m_DoW = DaysOfWeek.Sun;
//    #endregion

//    #region Command Line Switches
//    /// <summary>Simple example of a Boolean switch.</summary>
//    [CommandLineSwitch("SomeHelp", "Show some additional help")]
//    public bool ShowSomeHelp
//    {
//        get { return m_showHelp; }
//        set { m_showHelp = value; }
//    }

//    /// <summary>Simple example of a Boolean switch.</summary>
//    /// <remark>There is no get value set, so this value is in effect
//    /// a write only one.  This can affect the implementation of the toggle
//    /// for Boolean values.</remark>
//    [CommandLineSwitch("SomethingElse", "Do something else")]
//    public bool Wibble
//    {
//        set { m_SomethingElse = value; }
//    }

//    /// <summary>Simple example of a string switch with an alias.</summary>
//    [CommandLineSwitch("Name", "User Name")]
//    [CommandLineAlias("User")]
//    public string UserName
//    {
//        get { return m_SomeName; }
//        set { m_SomeName = value; }
//    }

//    /// <summary>Simple example of an integer switch.</summary>
//    [CommandLineSwitch("Age", "User age")]
//    public int Age
//    {
//        get { return m_Age; }
//        set { m_Age = value; }
//    }

//    /// <summary>Simple example of a read-only, e.g. no writeback, Boolean
//    /// command line switch.</summary>
//    [CommandLineSwitch("Test", "Test switch")]
//    public string Test
//    {
//        get { return m_test; }
//    }

//    [CommandLineSwitch("Day", "Day of the week selection")]
//    [CommandLineAlias("DoW")]
//    public DaysOfWeek DoW
//    {
//        get { return m_DoW; }
//        set { m_DoW = value; }
//    }
//    #endregion

//    #region Private Utility Functions
//    private int Run(string[] cmdLine)
//    {
//        // Initialise the command line parser, passing in a reference to this
//        // class so that we can look for any attributes that implement
//        // command line switches.
//        Parser parser = new Parser(System.Environment.CommandLine, this);

//        // Programmatically add some switches to the command line parser.
//        parser.AddSwitch("Wibble", "Do something silly");

//        // Add a switches with lots of aliases for the first name, "help" and "a".
//        parser.AddSwitch(new string[] { "help", @"\?" }, "show help");
//        parser.AddSwitch(new string[] { "a", "b", "c", "d", "e", "f" }, "Early alphabet");

//        // Parse the command line.
//        parser.Parse();

//        // ----------------------- DEBUG OUTPUT -------------------------------
//        Console.WriteLine("Program Name      : {0}", parser.ApplicationName);
//        Console.WriteLine("Non-switch Params : {0}", parser.Parameters.Length);
//        for (int j=0; j < parser.Parameters.Length; j++)
//            Console.WriteLine("                {0} : {1}", j, parser.Parameters[j]);
//        Console.WriteLine("----");
//        Console.WriteLine("Value of ShowSomeHelp    : {0}", ShowSomeHelp);
//        Console.WriteLine("Value of m_SomethingElse : {0}", m_SomethingElse);
//        Console.WriteLine("Value of UserName        : {0}", UserName);
//        Console.WriteLine("----");

//        // Walk through all of the registered switches getting the available
//        // information back out.
//        Parser.SwitchInfo[] si = parser.Switches;
//        if (si != null)
//        {
//            Console.WriteLine("There are {0} registered switches:", si.Length);
//            foreach (Parser.SwitchInfo s in si)
//            {
//                Console.WriteLine("Command : {0} - [{1}]", s.Name, s.Description);
//                Console.Write("Type    : {0} ", s.Type);

//                if (s.IsEnum)
//                {
//                    Console.Write("- Enums allowed (");
//                    foreach (string e in s.Enumerations)
//                        Console.Write("{0} ", e);
//                    Console.Write(")");
//                }
//                Console.WriteLine();

//                if (s.Aliases != null)
//                {
//                    Console.Write("Aliases : [{0}] - ", s.Aliases.Length);
//                    foreach (string alias in s.Aliases)
//                        Console.Write(" {0}", alias);
//                    Console.WriteLine();
//                }

//                Console.WriteLine("------> Value is : {0} (Without any callbacks {1})\n",
//                    s.Value != null ? s.Value : "(Unknown)",
//                    s.InternalValue != null ? s.InternalValue : "(Unknown)");
//            }
//        }
//        else
//            Console.WriteLine("There are no registered switches.");

//        // Test looking for a specificly named values.
//        Console.WriteLine("----");
//        if (parser["help"] != null)
//            Console.WriteLine("Request for help = {0}", parser["help"]);
//        else
//            Console.WriteLine("Request for help has no associated value.");
//        Console.WriteLine("User Name is {0}", parser["name"]);

//        // Note the difference between the parser and a callback value.
//        Console.WriteLine("The property of test (/test) is internally is read-only, " +
//                                "e.g. no update can be made by the parser:\n" +
//                                "   -- The indexer gives a value of : {0}\n" +
//                                "   -- Internally the parser has    : {1}",
//                                parser["test"],
//                                parser.InternalValue("test"));

//        // Test if the enumeration value has changed to Friday!
//        if (DoW == DaysOfWeek.Fri)
//            Console.WriteLine("\nYeah Friday.... PUB LUNCH TODAY...");

//        // For error handling, were any switches handled?
//        string[] unhandled = parser.UnhandledSwitches;
//        if (unhandled != null)
//        {
//            Console.WriteLine("\nThe following switches were not handled.");
//            foreach (string s in unhandled)
//                Console.WriteLine("  - {0}", s);
//        }

//        return 0;
//    }
//    #endregion

//    private static int Main(string[] cmdLine)
//    {
//        Application app = new Application();
//        return app.Run(cmdLine);
//    }
//}

