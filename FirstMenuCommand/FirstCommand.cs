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

        public enum ControlType
        {
            OpenFile,
            RenameClass,
        }

        private HashSet<string> hasHandleFileOpen = new HashSet<string>();
        private HashSet<string> hasHandleFileRenameClass = new HashSet<string>();

        private void GetProjectItems(ProjectItem item, ControlType controlType)
        {
            var projCount = item.ProjectItems.Count;
            var index = 0;
            foreach (ProjectItem pi in item.ProjectItems)
            {
                var path = GetFileNames(pi);
                if (pi.Name.EndsWith(".cs") &&
                    //(path.Contains(@"\Scripts\GameMain\") || path.Contains(@"\Scripts\MogoEngine\")
                    //|| path.Contains(@"\Scripts\GameResource\") || path.Contains(@"\Scripts\ACTSystem\")
                    //|| path.Contains(@"\Scripts\SerializableData\")//|| path.Contains(@"\Scripts\GameLoader\")
                    (
                    path.Contains(@"GameCode\GameMain\GameMain\") || path.Contains(@"GameCode\MogoEngine\MogoEngine\")
                    || path.Contains(@"GameCode\GameResource\GameResource\")// || path.Contains(@"GameCode\ACTSystem\ACTSystem\")
                    || path.Contains(@"GameCode\SerializableData\SerializableData\")//|| path.Contains(@"\GameCode\GameLoader\GameLoader\")
                      ))
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
                            foreach (CodeElement code in pi.FileCodeModel.CodeElements)
                            {
                                content += RenameClasses(code);
                                content += RenameAttrs(code);
                                content += RenameMethods(code);
                            }

                            Console.WriteLine(content);

                            pi.Save();
                            hasHandleFileRenameClass.Add(path);
                        }
                        //}
                    }
                }
                try
                {
                    Debug.WriteLine("   " + pi.Name + " " + (index++) + "/" + projCount);
                }
                catch { }
                GetProjectItems(pi, controlType);
            }
        }

        private HashSet<string> hasHandleClasses = new HashSet<string>();

        private string RenameClasses(CodeElement item)
        {
            try
            {
                if (hasHandleClasses.Contains(item.Name))
                    return "";
                if (item.Name.Contains("alien"))
                    return "";
            }
            catch
            {
                return "";
            }

            //var sb = new StringBuilder();
            RenameClass(item);
            try
            {
                Debug.WriteLine("   " + item.Name + " current");
                //hasHandleClasses.Add(item.Name);
            }
            catch { }
            var projCount = item.Children.Count;
            var index = 0;
            foreach (CodeElement code in item.Children)
            {
                var res = RenameClass(code);
                try
                {
                    Debug.WriteLine("   " + code.Name + " " + (index++) + "/" + projCount);
                }
                catch { }
                //sb.AppendLine(res);
            }
            //return sb.ToString();
            return "";
        }

        private string RenameClass(CodeElement item)
        {
            //var sb = new StringBuilder();
            try
            {
                if (item.Kind == vsCMElement.vsCMElementClass)
                {
                    //if (item.Name.Contains("ACTRuntimeSoundPlayer"))
                    //{
                    //}
                    if (hasHandleClasses.Contains(item.Name))
                        return "";
                    if (whiteList.Contains(item.Name))
                        return "";

                    if (item.Name.Contains("alien"))
                        return "";

                    CodeElement2 code2 = item as CodeElement2;
                    var one = nameCounter++;
                    var alien2 = "alien" + nameCounter++;
                    var three = nameCounter++;

                    hasHandleClasses.Add(item.Name);
                    var replacement = string.Format("alien{0}{1}alien{2}", one, item.Name.Insert(item.Name.Length / 2, alien2), three);
                    code2.RenameSymbol(replacement);
                    hasHandleClasses.Add(replacement);
                    //sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
                }
                //sb.AppendLine(RenameClasses(item));
                var resAttrs = RenameAttrs(item);
                var resMethods = RenameMethods(item);
                //sb.AppendLine(resAttrs);
                //sb.AppendLine(resMethods);

                //sb.AppendLine(item.Name + " " + item.Kind);
            }
            catch (Exception ex)
            {
                except += " error: " + ex.Message + "\n";// item.Name + 
            }
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
            try
            {
                if (item.Kind == vsCMElement.vsCMElementVariable || item.Kind == vsCMElement.vsCMElementParameter || item.Kind == vsCMElement.vsCMElementProperty)
                {
                    if (item.Name.Contains("alien"))
                        return "";
                    CodeElement2 code2 = item as CodeElement2;
                    var one = nameCounter++;
                    var alien2 = "alien" + nameCounter++;
                    var three = nameCounter++;

                    var replacement = string.Format("alien{0}{1}alien{2}", one, item.Name.Insert(item.Name.Length / 2, alien2), three);
                    code2.RenameSymbol(replacement);
                    //sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
                }
                else
                {

                }

                //sb.AppendLine(item.Name + " " + item.Kind);
            }
            catch (Exception ex)
            {
                except += " error: " + ex.Message + "\n";//item.Name + 
            }
            //return sb.ToString();
            return "";
        }

        private string RenameMethods(CodeElement item)
        {
            return "";

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
            try
            {
                if (item.Kind == vsCMElement.vsCMElementFunction)
                {
                    //if (item.Name == "Play")
                    //{

                    //}

                    if (methodWhiteList.Contains(item.Name) || item.Name.Contains("alien"))
                        return "";
                    CodeFunction2 code3 = item as CodeFunction2;
                    if (code3.OverrideKind == vsCMOverrideKind.vsCMOverrideKindOverride || code3.OverrideKind == vsCMOverrideKind.vsCMOverrideKindNew)
                        return "";
                    CodeElement2 code2 = item as CodeElement2;
                    var one = nameCounter++;
                    var alien2 = "alien" + nameCounter++;
                    var three = nameCounter++;

                    var replacement = string.Format("alien{0}{1}alien{2}", one, item.Name.Insert(item.Name.Length / 2, alien2), three);
                    code2.RenameSymbol(replacement);
                    //sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
                }
                else
                {

                }

                //sb.AppendLine(item.Name + " " + item.Kind);
            }
            catch (Exception ex)
            {
                except += " error: " + ex.Message + "\n";//item.Name + 
            }
            //return sb.ToString();
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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            hasHandleFileOpen.Clear();
            hasHandleFileRenameClass.Clear();
            var sw = new Stopwatch();
            sw.Start();
            //var dte = Template.AddNewItemWizard.EnvDTEHelper.GetIntegrityServiceInstance("DevProject.CSharp.Plugins");
            var dte = Template.AddNewItemWizard.EnvDTEHelper.GetIntegrityServiceInstance("ThirdPartyPlugins");

            foreach (Project item in dte.Solution.Projects)
            {
                var projCount = item.ProjectItems.Count;
                var index = 0;
                foreach (ProjectItem pi in item.ProjectItems)
                {
                    GetProjectItems(pi, ControlType.OpenFile);
                    Debug.WriteLine(pi.Name + " " + (index++) + "/" + projCount);
                }
            }
            foreach (Project item in dte.Solution.Projects)
            {
                var projCount = item.ProjectItems.Count;
                var index = 0;
                foreach (ProjectItem pi in item.ProjectItems)
                {
                    GetProjectItems(pi, ControlType.RenameClass);
                    Debug.WriteLine(pi.Name + " " + (index++) + "/" + projCount);
                }
            }
            //var s = sb.ToString();
            var time = sw.ElapsedMilliseconds / 1000;
            var min = time / 60;
            //Debug.WriteLine(s);
            Debug.WriteLine("nameCounter: " + nameCounter);
            Debug.WriteLine("second: " + time);
            Debug.WriteLine("min: " + min);
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
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
