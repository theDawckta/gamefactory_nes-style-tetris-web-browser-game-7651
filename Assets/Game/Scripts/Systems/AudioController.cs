using System;
using System.Collections;
using Game.Gameplay;
using UnityEngine;

namespace Game.Systems
{
    public class AudioController : MonoBehaviour
    {
        [SerializeField] private GameplayController gameplayController;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip pieceMoveSfx;
        [SerializeField] private AudioClip pieceLockSfx;
        [SerializeField] private AudioClip lineClearSfx;
        [SerializeField] private AudioClip levelUpSfx;
        [SerializeField] private AudioClip gameOverSfx;

        private int _lastLevel;

        private void Start()
        {
            if (musicSource != null)
            {
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            if (sfxSource != null)
                sfxSource.playOnAwake = false;

            if (gameplayController == null)
                return;

            gameplayController.OnStateChanged += OnGameStateChanged;
            gameplayController.OnPieceMoved += OnPieceMoved;
            gameplayController.OnPieceLocked += OnPieceLocked;
            gameplayController.OnLinesCleared += OnLinesCleared;

            PlayMusic(menuMusic);
        }

        private void OnDestroy()
        {
            if (gameplayController == null)
                return;

            gameplayController.OnStateChanged -= OnGameStateChanged;
            gameplayController.OnPieceMoved -= OnPieceMoved;
            gameplayController.OnPieceLocked -= OnPieceLocked;
            gameplayController.OnLinesCleared -= OnLinesCleared;
        }

        private void OnGameStateChanged()
        {
            string state = gameplayController.CurrentState;
            if (state == "Idle")
            {
                CrossFadeMusic(menuMusic);
            }
            else if (state == "Playing")
            {
                if (musicSource != null && musicSource.clip != gameplayMusic)
                {
                    _lastLevel = gameplayController.CurrentLevel;
                    CrossFadeMusic(gameplayMusic);
                }
            }
            else if (state == "GameOver")
            {
                StopMusicImmediate();
                if (sfxSource != null && gameOverSfx != null)
                    sfxSource.PlayOneShot(gameOverSfx);
            }
        }

        private void OnPieceMoved()
        {
            if (sfxSource != null && pieceMoveSfx != null)
                sfxSource.PlayOneShot(pieceMoveSfx);
        }

        private void OnPieceLocked(PieceState _)
        {
            if (sfxSource != null && pieceLockSfx != null)
                sfxSource.PlayOneShot(pieceLockSfx);
        }

        private void OnLinesCleared(int lines)
        {
            if (sfxSource != null && lineClearSfx != null)
                sfxSource.PlayOneShot(lineClearSfx);

            int currentLevel = gameplayController.CurrentLevel;
            if (currentLevel > _lastLevel && sfxSource != null && levelUpSfx != null)
                sfxSource.PlayOneShot(levelUpSfx);
            _lastLevel = currentLevel;
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
                return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        private void CrossFadeMusic(AudioClip clip)
        {
            StopAllCoroutines();
            StartCoroutine(CrossFadeCoroutine(clip));
        }

        private void StopMusicImmediate()
        {
            StopAllCoroutines();
            if (musicSource != null)
                musicSource.Stop();
        }

        private IEnumerator CrossFadeCoroutine(AudioClip newClip)
        {
            float fadeDuration = 0.5f;

            if (musicSource != null && musicSource.isPlaying)
            {
                float startVolume = musicSource.volume;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                musicSource.volume = 0f;
                musicSource.Stop();
            }

            if (musicSource != null && newClip != null)
            {
                musicSource.clip = newClip;
                musicSource.Play();

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    yield return null;
                }
                musicSource.volume = 1f;
            }
        }
    }
}
