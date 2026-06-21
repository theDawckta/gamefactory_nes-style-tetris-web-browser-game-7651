using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Gameplay;

public class LeaderboardServiceTests
{
    private GameObject _go;
    private LeaderboardService _service;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("LeaderboardService");
        _service = _go.AddComponent<LeaderboardService>();
        _service.ServerBaseUrl = "http://127.0.0.1:19999"; // unreachable port
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
    }

    [UnityTest]
    public IEnumerator GetScores_CallsOnError_WhenServerUnreachable()
    {
        string receivedError = null;
        bool callbackFired = false;

        _service.GetScores(
            onSuccess: _ => callbackFired = true,
            onError: err => { receivedError = err; callbackFired = true; }
        );

        float timeout = Time.realtimeSinceStartup + 10f;
        while (!callbackFired && Time.realtimeSinceStartup < timeout)
            yield return null;

        Assert.IsTrue(callbackFired, "Callback never fired");
        Assert.IsNotNull(receivedError, "onError should be called with an error message");
    }

    [UnityTest]
    public IEnumerator PostScore_CallsOnError_WhenServerUnreachable()
    {
        string receivedError = null;
        bool callbackFired = false;

        _service.PostScore(
            name: "AAA",
            score: 1000,
            onSuccess: _ => callbackFired = true,
            onError: err => { receivedError = err; callbackFired = true; }
        );

        float timeout = Time.realtimeSinceStartup + 10f;
        while (!callbackFired && Time.realtimeSinceStartup < timeout)
            yield return null;

        Assert.IsTrue(callbackFired, "Callback never fired");
        Assert.IsNotNull(receivedError, "onError should be called with an error message");
    }

    [UnityTest]
    public IEnumerator LeaderboardService_ComponentAttaches_ToGameObject()
    {
        yield return null;
        Assert.IsNotNull(_service);
        Assert.AreEqual("http://127.0.0.1:19999", _service.ServerBaseUrl);
    }
}
