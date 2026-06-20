using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[TestFixture]
public class QAIntegrationTests
{
    [UnityTest]
    public IEnumerator StartScreen_IsVisible_OnLoad()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(1f);
        Assert.IsNotNull(Object.FindAnyObjectByType<StartScreen>());
    }
}
