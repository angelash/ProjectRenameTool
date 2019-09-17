//------------------------------------------------------------------------------
// <copyright file="FirstCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FirstMenuCommand
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FirstCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("60b707cc-b2ad-418d-bad1-65a057597ee2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Searching Path
        /// </summary>
        private static readonly String SearchingPath = @"GameCode\ThirdPartyPlugins";
        //private static readonly String SearchingPath = @"RenameTest\CarFactory\";

        /// <summary>
        /// ProjectName
        /// </summary>
        //private static readonly String ProjectName = "ThirdPartyPlugins";
        private static readonly String ProjectName = "GameCode";
        //private static readonly String ProjectName = "CarFactory";

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private FirstCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FirstCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new FirstCommand(package);
        }

        //private string sb = "";
        private string except = "";
        private int nameCounter = 0;
        private string alien = "bt1zy";
        private string stringThis = "this";
        private string stringGlobal = "global";

        private int fileCount = 0;
        private int handleCount = 0;

        public enum ControlType
        {
            OpenFile,
            RenameClass,
        }

        private HashSet<string> hasHandleFileOpen = new HashSet<string>();
        private HashSet<string> hasHandleFileRenameClass = new HashSet<string>();
        private HashSet<string> interFaceFunction = new HashSet<string>();

        private int currIndex;
        ///HashSet<string> searchingPath = new HashSet<string>()
        String[] searchingPath = new String[]
        {
            @"GameCode\GameMain\GameMain\Adapters",//GameMain共400个文件 重命名5140个 用时18分钟 成功没报错
            @"GameCode\GameMain\GameMain\Common",
            @"GameCode\GameMain\GameMain\Entities",
            @"GameCode\GameMain\GameMain\Inspectors",
            @"GameCode\GameMain\GameMain\MonoBehaviours",
            @"GameCode\GameMain\GameMain\SubSystems",
            @"GameCode\MogoEngine\MogoEngine\", //75文件 成功没报错
            //@"GameCode\ThirdPartyPlugins", //忽视
            @"GameCode\GameResource\GameResource\",//28文件 重命名332个 成功没报错
            @"GameCode\ACTSystem\ACTSystem\",//76文件 重命名1052个 成功没报错
            //@"GameCode\SerializableData\SerializableData\", //忽视
            @"GameCode\GameLoader\GameLoader\", //103文件 重命名1302个 成功没报错
            //@"\Scripts\GameMain\",
            //@"\Scripts\MogoEngine\",
            //@"\Scripts\GameResource\",
            //@"\Scripts\ACTSystem\",
            //@"\Scripts\SerializableData\",
            //@"\Scripts\GameLoader\",
        };
        String[] projectName = new String[]
        {
            "GameMain",
            "GameMain",
            "GameMain",
            "GameMain",
            "GameMain",
            "GameMain",
            "MogoEngine",
            //"ThirdPartyPlugins",
            "GameResource",
            "ACTSystem",
            //"SerializableData",
            "GameLoader",
        };


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            //var dte = Template.AddNewItemWizard.EnvDTEHelper.GetIntegrityServiceInstance("DevProject.CSharp.Plugins");
            for (int i = 0; i < searchingPath.Length; i++)
            {
                interFaceFunction.Clear();
                hasHandleFileOpen.Clear();
                hasHandleFileRenameClass.Clear();
                currIndex = i;
                var dte = Template.AddNewItemWizard.EnvDTEHelper.GetIntegrityServiceInstance(projectName[currIndex]);
                foreach (Project item in dte.Solution.Projects)
                {
                    var projCount = item.ProjectItems.Count;
                    var index = 0;
                    foreach (ProjectItem pi in item.ProjectItems)
                    {
                        GetProjectItems(pi, ControlType.OpenFile);
                    }
                }
                foreach (Project item in dte.Solution.Projects)
                {
                    var projCount = item.ProjectItems.Count;
                    var index = 0;
                    foreach (ProjectItem pi in item.ProjectItems)
                    {
                        GetProjectItems(pi, ControlType.RenameClass);
                    }

                }
                GC.Collect();
            }
            //for (int i = 0; i < searchingPath.Length; i++)
            //{
            //    currIndex = i;
            //    var dte = Template.AddNewItemWizard.EnvDTEHelper.GetIntegrityServiceInstance(projectName[currIndex]);

            //    //GC.Collect();
            //}
            //var s = sb.ToString();
            var time = sw.ElapsedMilliseconds / 1000;
            var min = time / 60;
            //Debug.WriteLine(s);
            Debug.WriteLine("nameCounter: " + nameCounter / 3);
            Debug.WriteLine("second: " + time);
            Debug.WriteLine("min: " + min);
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback() /n SearchingPath:{1}", this.GetType().FullName, SearchingPath);
            string title = "FirstCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void GetProjectItems(ProjectItem item, ControlType controlType)
        {
            var projCount = item.ProjectItems.Count;
            var index = 0;
            foreach (ProjectItem pi in item.ProjectItems)
            {
                var path = GetFileNames(pi);
                if (pi.Name.EndsWith(".cs") && path.Contains(searchingPath[currIndex]))
                {

                    if (controlType == ControlType.OpenFile)
                    {
                        if (!hasHandleFileOpen.Contains(path))
                        {
                            Window wnd = pi.Open(EnvDTE.Constants.vsViewKindPrimary);
                            if (!wnd.Visible)
                            {
                                wnd.Visible = true;
                                wnd.Activate();
                            }
                            Print(path + " --- has open");
                            fileCount++;
                            hasHandleFileOpen.Add(path);
                        }
                    }
                    else if (controlType == ControlType.RenameClass)
                    {
                        if (!hasHandleFileRenameClass.Contains(path))
                        {
                            var content = "";
                            //sb += pi.Name + " files: " + path + "\n";
                            //if (path.Contains("ACTSystemAdapter"))
                            //{
                            Print("Start Handle:" + pi.Name);
                            foreach (CodeElement code in pi.FileCodeModel.CodeElements)
                            {
                                content += RenameClasses(code);
                                content += RenameAttrs(code);
                                content += RenameMethods(code);
                            }

                            //Console.WriteLine(content);
                            handleCount++;
                            pi.Save();
                            Print(path + " --- has handle Progress:" + handleCount + "/" + fileCount);
                            hasHandleFileRenameClass.Add(path);
                        }
                        GC.Collect();
                        //}
                    }
                }

                //Debug.WriteLine("Name:" + pi.Name + "  Index:" + ((index++)+1) + "/" + projCount);

                GetProjectItems(pi, controlType);
            }
        }

        private HashSet<string> hasHandleClasses = new HashSet<string>();

        private string RenameClasses(CodeElement item)
        {
            //try
            //{
            if (item.Kind == vsCMElement.vsCMElementImportStmt)
                return "";
            CodeClass2 codeClass2 = item as CodeClass2;
            //if (hasHandleClasses.Contains(item.Name) && codeClass2.ClassKind != vsCMClassKind.vsCMClassKindPartialClass)
            //    return "";
            //if (item.Name.Contains(alien))
            //    return "";
            PrintItemInfo(item);
            //var sb = new StringBuilder();
            RenameClass(item);
            //Debug.WriteLine("RenameClasses.FullName:" + item.FullName);
            var projCount = item.Children.Count;
            var index = 0;
            foreach (CodeElement code in item.Children)
            {
                var res = RenameClass(code);
            }
            //}
            //catch { }
            //return sb.ToString();
            return "";
        }

        private string RenameClass(CodeElement item)
        {
            //var sb = new StringBuilder();
            //try
            //{
            if (item.Kind == vsCMElement.vsCMElementClass)
            {
                //if (item.Name.Contains("ACTRuntimeSoundPlayer"))
                //{
                //}
                CodeClass2 codeClass2 = item as CodeClass2;
                PrintClass2Info(codeClass2);
                //if (hasHandleClasses.Contains(item.Name) && codeClass2.ClassKind != vsCMClassKind.vsCMClassKindPartialClass)
                //    return "";
                //if (whiteList.Contains(item.Name))
                //    return "";
                //if (item.Name.Contains(alien) && codeClass2.ClassKind != vsCMClassKind.vsCMClassKindPartialClass)
                //    return "";
                if (codeClass2.IsShared)
                {
                    PrintDetial("StaticClass :" + item.Name);
                    return "";
                }
                if (codeClass2.Name.Contains(stringGlobal))
                {
                    PrintDetial("GlobalClass :" + item.Name);
                    return "";
                }

                if (codeClass2.ImplementedInterfaces.Count > 0)
                {
                    foreach (CodeInterface2 codeE in codeClass2.ImplementedInterfaces)
                    {

                        if (codeE.Members != null && codeE.Members.Count > 0)
                        {
                            if (codeE.Bases != null)
                            {
                                PrintDetial("实现了非基接口类 :" + item.Name);
                                return "";
                            }
                            foreach (CodeElement codeE2 in codeE.Members)
                            {
                                if (codeE2.Kind == vsCMElement.vsCMElementFunction)
                                    Debug.WriteLine("interface name:" + codeE.Name + "." + codeE2.Name);
                                interFaceFunction.Add(codeE.Name + "." + codeE2.Name);
                                interFaceFunction.Add(codeE2.Name);
                            }
                        }
                    }
                }

                CodeElement2 code2 = item as CodeElement2;
                try
                {
                    var one = nameCounter++;
                    var alien2 = alien + nameCounter++;
                    var three = nameCounter++;
                    PrintDetial("RenameClass :" + item.Name);
                    hasHandleClasses.Add(item.Name);
                    var randomOne = GetRandomName(3, 5, useUpper: true);
                    var randomThree = GetRandomName(3, 5, useUpper: true);
                    var replacement = string.Format("{0}{1}{2}{3}{4}", randomOne, one, item.Name.Insert(item.Name.Length / 2, alien2), randomThree, three);
                    code2.RenameSymbol(replacement);
                    hasHandleClasses.Add(replacement);
                }
                catch (Exception ex)
                {
                    except += " error: " + ex.Message + "\n" + item.Name;
                    PrintClass2Info(codeClass2);
                }
                //sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
                var resAttrs = RenameAttrs(item);
                var resMethods = RenameMethods(item);
            }
            //sb.AppendLine(RenameClasses(item));

            //sb.AppendLine(resAttrs);
            //sb.AppendLine(resMethods);

            //sb.AppendLine(item.Name + " " + item.Kind);
            //}
            //catch (Exception ex)
            //{
            //    except += " error: " + ex.Message + "\n";// item.Name + 
            //}
            //return sb.ToString();
            return "";
        }

        private string RenameAttrs(CodeElement item)
        {
            //var sb = new StringBuilder();
            var res = RenameAttr(item);
            //sb.AppendLine(res);
            foreach (CodeElement code in item.Children)
            {
                var res1 = RenameAttr(code);
                //sb.AppendLine(res1);
            }
            //return sb.ToString();
            return "";
        }

        private string RenameAttr(CodeElement item)
        {
            //var sb = new StringBuilder();
            //try
            //{
            if (item.Kind == vsCMElement.vsCMElementVariable || item.Kind == vsCMElement.vsCMElementParameter || item.Kind == vsCMElement.vsCMElementProperty)
            {
                if (item.Name.Contains(alien))
                {
                    PrintDetial("AttrNameHasAlien :" + item.Name);
                    return "";
                }
                if (item.Name.Contains(stringThis))
                {
                    PrintDetial("AttrNameHasThis :" + item.Name);
                    return "";
                }
                if (item.Name.Contains(stringGlobal))
                {
                    PrintDetial("GlobalAttr :" + item.Name);
                    return "";
                }
                if (interFaceFunction.Contains(item.Name))
                {
                    PrintDetial("InterFaceAttr :" + item.Name);
                    return "";
                }
                CodeProperty2 CodeProperty = item as CodeProperty2;
                if (CodeProperty != null && CodeProperty.OverrideKind != null && (CodeProperty.OverrideKind == vsCMOverrideKind.vsCMOverrideKindOverride
                    || CodeProperty.OverrideKind == vsCMOverrideKind.vsCMOverrideKindNew
                    || CodeProperty.OverrideKind == vsCMOverrideKind.vsCMOverrideKindAbstract
                    || CodeProperty.OverrideKind == vsCMOverrideKind.vsCMOverrideKindVirtual))
                {
                    PrintDetial("AttrIsSpecial :" + item.Name);
                    return "";
                }
                CodeVariable2 codeVariable = item as CodeVariable2;
                if ((CodeProperty != null && CodeProperty.IsShared)
                    || (codeVariable != null && codeVariable.IsShared))
                {
                    PrintDetial("StaticAttr :" + item.Name);
                    return "";
                }
                try
                {
                    CodeElement2 code2 = item as CodeElement2;
                    var one = nameCounter++;
                    var alien2 = alien + nameCounter++;
                    var three = nameCounter++;
                    PrintDetial("RenameAttribute :" + item.Name);
                    var randomOne = GetRandomName(3, 5, useUpper: true);
                    var randomThree = GetRandomName(3, 5, useUpper: true);
                    var replacement = string.Format("{0}{1}{2}{3}{4}", randomOne, one, item.Name.Insert(item.Name.Length / 2, alien2), randomThree, three);
                    code2.RenameSymbol(replacement);
                }
                catch (Exception ex)
                {
                    except += " error: " + ex.Message + "\n" + item.Name;
                    PrintProperty2Info(CodeProperty);
                }
                //sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
            }
            else
            {

            }

            return "";
        }

        private string RenameMethods(CodeElement item)
        {
            //return "";
            //var sb = new StringBuilder();
            var res = RenameMethod(item);
            //sb.AppendLine(res);
            foreach (CodeElement code in item.Children)
            {

                var res1 = RenameMethod(code);
                //sb.AppendLine(res1);
            }
            //return sb.ToString();
            return "";
        }

        private string RenameMethod(CodeElement item)
        {
            //var sb = new StringBuilder();
            if (item.Kind == vsCMElement.vsCMElementFunction)
            {
                //Debug.WriteLine("RenameMethod.FullName:" + item.FullName);
                if (methodWhiteList.Contains(item.Name) || interFaceFunction.Contains(item.Name) || item.Name.Contains(alien))
                {
                    Print("WriteListFunction :" + item.Name);
                    return "";
                }
                try
                {
                    CodeFunction2 code3 = item as CodeFunction2;
                    PrintFunction2Info(code3);
                    if (code3.OverrideKind == vsCMOverrideKind.vsCMOverrideKindOverride
                        || code3.OverrideKind == vsCMOverrideKind.vsCMOverrideKindNew
                        || code3.OverrideKind == vsCMOverrideKind.vsCMOverrideKindAbstract
                        || code3.OverrideKind == vsCMOverrideKind.vsCMOverrideKindVirtual)
                    {
                        PrintDetial("SpecialFunction :" + item.Name);
                        return "";
                    }
                    if (code3.IsShared)
                    {
                        PrintDetial("StaticFunction :" + item.Name);
                        return "";
                    }
                    if (item.Name.Contains(stringGlobal))
                    {
                        PrintDetial("GlobalFunction :" + item.Name);
                        return "";
                    }

                    CodeElement2 code2 = item as CodeElement2;
                    var one = nameCounter++;
                    var alien2 = alien + nameCounter++;
                    var three = nameCounter++;
                    PrintDetial("RenameFunction :" + item.Name);
                    var randomOne = GetRandomName(3, 5, useUpper: true);
                    var randomThree = GetRandomName(3, 5, useUpper: true);
                    var replacement = string.Format("{0}{1}{2}{3}{4}", randomOne, one, item.Name.Insert(item.Name.Length / 2, alien2), randomThree, three);
                    code2.RenameSymbol(replacement);

                    //sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
                }
                catch (Exception ex)
                {
                    //except += " error: " + ex.Message + "\n" + item.Name;
                    CodeFunction2 code3 = item as CodeFunction2;
                    PrintFunction2Info(code3);
                }
            }
            else
            {

            }
            return "";
        }

        private string GetFileNames(ProjectItem item)
        {
            var sb = new StringBuilder();

            for (short i = 0; i < item.FileCount; i++)
            {
                sb.Append(item.FileNames[i] + ",");
            }
            return sb.ToString();
        }


        private static string[] vc = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                                                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
        private static string[] nums = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public static System.Random GetRandom()
        {
            System.Threading.Thread.Sleep(1);
            long tick = DateTime.Now.Ticks;
            return new System.Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
        }

        public static string GetRandomName(int min, int max, bool isFirstNum = false, bool isLastNum = false, bool useUpper = false)
        {
            var random = GetRandom();
            var count = random.Next(min, max);
            var result = new StringBuilder();

            if (isFirstNum)
                result.Append(nums[random.Next(0, 9)]);

            var vcCount = useUpper ? 51 : 25;
            for (int i = 0; i < count; i++)
            {
                result.Append(vc[random.Next(0, vcCount)]);
            }
            if (isLastNum)
                result.Append(nums[random.Next(0, 9)]);
            return result.ToString();
        }

        public static void Print(String str)
        {
            //return;
            Debug.WriteLine(str);
        }

        public static void PrintDetial(String str)
        {
            return;
            Debug.WriteLine(str);
        }

        public static void PrintItemInfo(CodeElement item)
        {
            return;
            Debug.WriteLine("------------PrintItemInfo----------");
            Debug.WriteLine("Rename Classes.InfoLocation:" + item.InfoLocation);
            Debug.WriteLine("Rename Classes.IsCodeType:" + item.IsCodeType);
            Debug.WriteLine("Rename Classes.Kind:" + item.Kind);
            Debug.WriteLine("Rename Classes.Language:" + item.Language);
            Debug.WriteLine("Rename Classes.StartPoint:" + item.StartPoint);
            Debug.WriteLine("Rename Classes.EndPoint:" + item.EndPoint);
            Debug.WriteLine("Rename Classes.FullName:" + item.FullName);
            Debug.WriteLine("Rename Classes.Name:" + item.Name);
            Debug.WriteLine("-----------------------------------");
        }

        public static void PrintClass2Info(CodeClass2 item)
        {
            return;
            Debug.WriteLine("----------PrintClass2Info--------");
            Debug.WriteLine("Rename Classes.FullName:" + item.FullName);
            Debug.WriteLine("Rename Classes.ClassKind:" + item.ClassKind);
            Debug.WriteLine("Rename Classes.Kind:" + item.Kind);
            Debug.WriteLine("Rename Classes.InheritanceKind:" + item.InheritanceKind);
            Debug.WriteLine("Rename Classes.Parts:" + item.Parts);
            Debug.WriteLine("Rename Classes.IsShared:" + item.IsShared);
            Debug.WriteLine("Rename Classes.IsAbstract:" + item.IsAbstract);
            Debug.WriteLine("Rename Classes.Members:" + item.Members);
            Debug.WriteLine("Rename Classes.PartialClasses:" + item.PartialClasses);
            Debug.WriteLine("Rename Classes.Name:" + item.Name);
            Debug.WriteLine("-----------------------------------");
        }

        public static void PrintAttribute2Info(CodeAttribute2 item)
        {
            return;
            Debug.WriteLine("--------PrintFunction2Info--------");
            Debug.WriteLine("Rename Method.FullName:" + item.FullName);
            Debug.WriteLine("-----------------------------------");
        }

        public static void PrintVariable2Info(CodeVariable2 item)
        {
            return;
            Debug.WriteLine("--------PrintVariable2Info--------");
            Debug.WriteLine("Rename Method.FullName:" + item.FullName);
            Debug.WriteLine("Rename Method.ConstKind :" + item.ConstKind);
            Debug.WriteLine("Rename Method.DocComment  :" + item.DocComment);
            Debug.WriteLine("Rename Method.IsCodeType  :" + item.IsCodeType);
            Debug.WriteLine("Rename Method.IsConstant  :" + item.IsConstant);
            Debug.WriteLine("Rename Method.IsGeneric  :" + item.IsGeneric);
            Debug.WriteLine("Rename Method.IsShared  :" + item.IsShared);
            Debug.WriteLine("Rename Method.Kind  :" + item.Kind);
            Debug.WriteLine("Rename Method.Prototype   :" + item.Prototype);
            Debug.WriteLine("Rename Method.Type   :" + item.Type);
            Debug.WriteLine("Rename Method.Name  :" + item.Name);
            Debug.WriteLine("-----------------------------------");
        }

        public static void PrintParameter2Info(CodeParameter2 item)
        {
            return;
            Debug.WriteLine("--------PrintParameter2Info--------");
            Debug.WriteLine("Rename Method.FullName:" + item.FullName);
            Debug.WriteLine("Rename Method.DefaultValue  :" + item.DefaultValue);
            Debug.WriteLine("Rename Method.DocComment   :" + item.DocComment);
            Debug.WriteLine("Rename Method.ParameterKind   :" + item.ParameterKind);
            Debug.WriteLine("Rename Method.Type   :" + item.Type);
            Debug.WriteLine("Rename Method.Name  :" + item.Name);
            Debug.WriteLine("-----------------------------------");
        }

        public static void PrintProperty2Info(CodeProperty2 item)
        {
            return;
            Debug.WriteLine("--------PrintParameter2Info--------");
            Debug.WriteLine("Rename Method.FullName:" + item.FullName);
            Debug.WriteLine("Rename Method.OverrideKind   :" + item.OverrideKind);
            Debug.WriteLine("-----------------------------------");
        }

        public static void PrintFunction2Info(CodeFunction2 item)
        {
            return;
            Debug.WriteLine("--------PrintFunction2Info--------");
            Debug.WriteLine("Rename Method.FullName:" + item.FullName);
            Debug.WriteLine("Rename Method.CanOverride:" + item.CanOverride);
            Debug.WriteLine("Rename Method.ExtenderNames:" + item.ExtenderNames);
            Debug.WriteLine("Rename Method.FunctionKind:" + item.FunctionKind);
            Debug.WriteLine("Rename Method.InfoLocation:" + item.InfoLocation);
            Debug.WriteLine("Rename Method.IsGeneric:" + item.IsGeneric);
            Debug.WriteLine("Rename Method.IsOverloaded:" + item.IsOverloaded);
            Debug.WriteLine("Rename Method.IsShared:" + item.IsShared);
            Debug.WriteLine("Rename Method.Kind:" + item.Kind);
            Debug.WriteLine("Rename Method.Language:" + item.Language);
            Debug.WriteLine("Rename Method.MustImplement:" + item.MustImplement);
            Debug.WriteLine("Rename Method.Overloads:" + item.Overloads);
            Debug.WriteLine("Rename Method.OverrideKind:" + item.OverrideKind);
            Debug.WriteLine("Rename Method.Prototype:" + item.Prototype);
            Debug.WriteLine("Rename Method.Type:" + item.Type);
            Debug.WriteLine("-----------------------------------");
        }

        HashSet<string> whiteList = new HashSet<string>()
        {
            //ACTSystem
            "ACTActor",

            //GameLoader
            "FileAccessManager",
            "ProgressBar",
            "SystemConfig",
            "PlatformSdkMgr",

            //MogoEngine
            "Entity",
            "EngineDriver",
            "MogoWorld",
            "SceneMgr",
            "LoadSceneSetting",

            //GameMain
            "XBaseScrollRect",
            "XScrollRect",
            "ModelDragComponent",
            "ActModelComponent",
            "XIcon",
            "Main",
            "XContainer",
            "LuaFacade",
            "LuaGameState",
            "LuaPreloadPathsBuilder",
            "LuaBehaviour",

            "LuaUIComponent",
            "LuaUIDragable",
            "LuaUIPointable",
            "LuaUIPointableDragable",
            "LuaUIPanel",
            "LuaUIList",
            "LuaUIComplexList",
            "LuaUIListItem",
            "LuaUIListDirection",
            "LuaUIPageableList",
            "LuaUIScrollPage",
            "LuaUIScrollView",
            "LuaUIToggleGroup",
            "LuaUIToggle",
            "LuaUIProgressBar",
            "LuaUIScaleProgressBar",
            "LuaUINavigationMenu",
            "LuaUINavigationMenuItem",
            "LuaUIParticle",
            "LuaUIInputField",
            "LuaUIInputFieldMesh",
            "LuaUILinkTextMesh",
            "LuaUIChatEmojiItem",
            "LuaUIChatTextBlock",
            "LuaUIVoiceDriver",
            "LuaUIMultipleProgressBar",
            "LuaUISlider",
            "LuaUIRoller",
            "LuaUIRollerItem",
            "LuaUIRotateModel",

            "SortOrderedRenderAgent",

            "Locater",
            "Resizer",
            "Rotator",
            "XArtNumber",
            "XButtonHitArea",
            "XInvisibleButton",
            "XImageUpgradeFilling",
            "XImageFilling",
            "XImageScaleFilling",
            "XImageScaling",
            "XImageFloat",
            "XImageFollow",
            "XImageFlowLight",
            "XImageTweenAlpha",
            "XImageTweenColor",
            "XGameObjectTweenPosition",
            "XGameObjectTweenScale",
            "XGameObjectTweenRotation",
            "XGameObjectComplexIcon",
            "XGameObjectTweenSizeDelta",
            "XCanvasGroupTweenAlpha",
            "XContainerRotator",
            "XScreenShot",
            "XIntroduceBoss",
            "XIntroduceNormal",
            "XUILine",
            "XIconAnim",
            "XIconAnimOnce",
            "ActorModelComponent",
            "EquipModelComponent",
            "PathModelComponent",
            "PlayerModelComponent",
            "XSkillDragContainer",
            "XImageChanger",
            "XLogoImage",
            "XHPBar",
            "XStickControl",

            "EntityLuaBase",
            "EntityCreature",
            "EntityPlayer",
            "EntityBillboard",
            "EntityStatic",
            "EntityDropItems",
            "PosPointingArrow",
            "EntityPointingArrow",
            "PosBubbleDialog",
            "EntityBubbleDialog",
            "GameObjectPoolManager",
            "ServerProxyFacade",
            "XResourceDownloadProgress",
            "XMapPathPoint",
            "XLowHPContainer",

            "LuaTween",
            "LuaUIRawImage",
            "LuaUIDial",
            "LuaUIGridLayoutGroup",
            "LuaUIGridLayoutGroupItem",
            "LuaUILayoutElement",
            "XCutoutAdapter",
            "XDamageHandler",
            "XAutoCombat",
            "XPhoto",
            "X3DGroup",
            "X3DGroupItem",

        };

        HashSet<string> methodWhiteList = new HashSet<string>() {
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnConnectedToServer",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDisconnectedFromServer",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnFailedToConnect",
            "OnFailedToConnectToMasterServer",
            "OnGUI",
            "OnJointBreak",
            "OnJointBreak2D",
            "OnMasterServerEvent",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnNetworkInstantiate",
            "OnParticleCollision",
            "OnParticleSystemStopped",
            "OnParticleTrigger",
            "OnPlayerConnected",
            "OnPlayerDisconnected",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRenderImage",
            "OnRenderObject",
            "OnSerializeNetworkView",
            "OnServerInitialized",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update",
        };

    }
}
