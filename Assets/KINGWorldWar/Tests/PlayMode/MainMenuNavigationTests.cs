using System.Collections;
using KingWorldWar.App;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.Scenes;
using KingWorldWar.Core.UI;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.UI.Common;
using KingWorldWar.UI.Modals;
using KingWorldWar.UI.Shell;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class MainMenuNavigationTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ServiceContainer.Instance.Reset();
            PlayerPrefs.DeleteKey(LocalizationService.LanguagePrefKey);

            var loadOp = SceneManager.LoadSceneAsync(SceneNameCatalog.MainMenu, LoadSceneMode.Single);
            while (!loadOp.isDone)
            {
                yield return null;
            }
            yield return null; // wait one frame for Start() to execute
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ServiceContainer.Instance.Reset();
            PlayerPrefs.DeleteKey(LocalizationService.LanguagePrefKey);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMenuScope_InitializesAllScreensAndStartsOnWorld()
        {
            var scope = Object.FindFirstObjectByType<MainMenuScope>();
            Assert.IsNotNull(scope, "MainMenuScope must exist in MainMenu.unity");

            var shell = scope.ShellView;
            Assert.IsNotNull(shell, "MainMenuShellView must be assigned in MainMenuScope");
            Assert.IsNotNull(shell.Navigation, "Navigation service must be bound to shell");
            Assert.AreEqual(ScreenId.World, shell.Navigation.CurrentScreen, "Initial screen must be World");
            Assert.IsFalse(shell.Navigation.CanGoBack, "At root World screen, CanGoBack must be false");

            yield return null;
        }

        [UnityTest]
        public IEnumerator TabNavigation_SwitchingTabs_PushesScreensAndShowsBackButton()
        {
            var scope = Object.FindFirstObjectByType<MainMenuScope>();
            var shell = scope.ShellView;
            var nav = shell.Navigation;

            // Navigate to Base
            nav.Push(ScreenId.Base);
            yield return null;

            Assert.AreEqual(ScreenId.Base, nav.CurrentScreen);
            Assert.IsTrue(nav.CanGoBack);
            Assert.AreEqual(2, nav.StackDepth);

            // Navigate to Army
            nav.Push(ScreenId.Army);
            yield return null;

            Assert.AreEqual(ScreenId.Army, nav.CurrentScreen);
            Assert.AreEqual(3, nav.StackDepth);

            yield return null;
        }

        [UnityTest]
        public IEnumerator BackButton_PopsNavigationStack_RestoresPreviousScreen()
        {
            var scope = Object.FindFirstObjectByType<MainMenuScope>();
            var shell = scope.ShellView;
            var nav = shell.Navigation;

            nav.Push(ScreenId.Alliance);
            yield return null;
            nav.Push(ScreenId.Rankings);
            yield return null;

            Assert.AreEqual(ScreenId.Rankings, nav.CurrentScreen);

            // Go back once -> Alliance
            shell.HandleBackTrigger();
            yield return null;
            Assert.AreEqual(ScreenId.Alliance, nav.CurrentScreen);

            // Go back once -> World (root)
            shell.HandleBackTrigger();
            yield return null;
            Assert.AreEqual(ScreenId.World, nav.CurrentScreen);
            Assert.IsFalse(nav.CanGoBack);

            yield return null;
        }

        [UnityTest]
        public IEnumerator LanguageToggle_SwitchesLanguageAndUpdatesUI()
        {
            var scope = Object.FindFirstObjectByType<MainMenuScope>();
            var shell = scope.ShellView;
            var loc = shell.Localization;

            Assert.IsNotNull(loc);
            Language initialLang = loc.CurrentLanguage;

            // Toggle language
            Language targetLang = (initialLang == Language.Arabic) ? Language.English : Language.Arabic;
            loc.SetLanguage(targetLang);
            yield return null;

            Assert.AreEqual(targetLang, loc.CurrentLanguage);

            // Toggle back
            loc.SetLanguage(initialLang);
            yield return null;

            Assert.AreEqual(initialLang, loc.CurrentLanguage);
        }

        [UnityTest]
        public IEnumerator ModalDialog_ShowsAndDismissesCorrectly()
        {
            var scope = Object.FindFirstObjectByType<MainMenuScope>();
            var modalView = scope.ModalView;
            Assert.IsNotNull(modalView);

            var modalService = ServiceContainer.Instance.Resolve<IModalService>();
            Assert.IsNotNull(modalService);

            bool confirmed = false;
            modalService.ShowConfirm(
                title: "Test Alert",
                message: "Test modal message",
                onConfirm: () => confirmed = true
            );
            yield return null;

            Assert.IsTrue(modalView.IsVisible, "Modal view must be visible when modal request is active");
            Assert.IsTrue(modalService.IsModalOpen);

            // Dismiss via service
            modalService.DismissCurrent();
            yield return null;

            Assert.IsFalse(modalView.IsVisible, "Modal view must close when dismissed");
            Assert.IsFalse(modalService.IsModalOpen);
        }

        [UnityTest]
        public IEnumerator SafeAreaFitter_AppliesWithoutThrowing()
        {
            var fitter = Object.FindFirstObjectByType<SafeAreaFitter>();
            Assert.IsNotNull(fitter);

            Assert.DoesNotThrow(() => fitter.ApplySafeArea());
            yield return null;
        }

        [Test]
        public void KwwButton_EnforcesMinimumTouchTarget_ClampsSmallSize()
        {
            var go = new GameObject("TestBtn");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24f, 20f);
            go.AddComponent<Image>();
            go.AddComponent<Button>();

            var kww = go.AddComponent<KwwButton>();
            kww.EnforceMinimumTouchTarget();

            Assert.GreaterOrEqual(rt.sizeDelta.x, KwwButton.MinTouchTargetWidth);
            Assert.GreaterOrEqual(rt.sizeDelta.y, KwwButton.MinTouchTargetHeight);

            Object.DestroyImmediate(go);
        }
    }
}
