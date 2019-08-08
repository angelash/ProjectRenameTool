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

        private string sb = "";
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
            foreach (ProjectItem pi in item.ProjectItems)
            {
                var path = GetFileNames(pi);
                if (pi.Name.EndsWith(".cs") && (path.Contains(@"\GameCode\GameMain\GameMain\") || path.Contains(@"\GameCode\MogoEngine\MogoEngine\")
                    || path.Contains(@"\GameCode\GameResource\GameResource\") || path.Contains(@"\GameCode\ACTSystem\ACTSystem\")
                     || path.Contains(@"\GameCode\GameLoader\GameLoader\") || path.Contains(@"\GameCode\SerializableData\SerializableData\")))
                {
                    if (controlType == ControlType.OpenFile)
                    {
                        if(!hasHandleFileOpen.Contains(path))
                        {
                            Window wnd = pi.Open(EnvDTE.Constants.vsViewKindPrimary);
                            wnd.Visible = true;
                            wnd.Activate();
                            hasHandleFileOpen.Add(path);
                        }
                    }
                    else if (controlType == ControlType.RenameClass)
                    {
                        if (!hasHandleFileRenameClass.Contains(path))
                        {
                            sb += pi.Name + " files: " + path + "\n";
                            foreach (CodeElement code in pi.FileCodeModel.CodeElements)
                            {
                                GetCodeElements(code);
                            }

                            pi.Save();
                            hasHandleFileRenameClass.Add(path);
                        }
                    }
                }
                GetProjectItems(pi, controlType);
            }
        }

        private void GetCodeElements(CodeElement item)
        {
            foreach (CodeElement code in item.Children)
            {
                try
                {
                    if (code.Kind == vsCMElement.vsCMElementClass)
                    {
                        if (whiteList.Contains(code.Name) || code.Name.Contains("alien"))
                            continue;
                        CodeElement2 code2 = code as CodeElement2;
                        var one = nameCounter++;
                        var alien2 = "alien" + nameCounter++;
                        var three = nameCounter++;

                        var replacement = string.Format("alien{0}{1}alien{2}", one, code.Name.Insert(code.Name.Length / 2, alien2), three);
                        code2.RenameSymbol(replacement);
                        sb += ("    " + code.Name + " " + code.IsCodeType + " " + code.Kind + "\n");
                    }
                }
                catch (Exception ex)
                {
                    except += item.Name + " error: " + ex.Message + "\n";
                }
            }
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
            var sw = new Stopwatch();
            sw.Start();
            var dte = Template.AddNewItemWizard.EnvDTEHelper.GetIntegrityServiceInstance();

            foreach (Project item in dte.Solution.Projects)
            {
                foreach (ProjectItem pi in item.ProjectItems)
                {
                    GetProjectItems(pi, ControlType.OpenFile);
                }
            }
            foreach (Project item in dte.Solution.Projects)
            {
                foreach (ProjectItem pi in item.ProjectItems)
                {
                    GetProjectItems(pi, ControlType.RenameClass);
                }
            }
            var s = sb.ToString();
            var time = sw.ElapsedMilliseconds;
            Console.WriteLine(s);
            Console.WriteLine(nameCounter);
            Console.WriteLine(time);

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

    }
}
