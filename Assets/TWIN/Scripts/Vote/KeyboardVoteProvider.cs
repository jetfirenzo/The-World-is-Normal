using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TWIN.Vote
{
    /// <summary>
    /// Desktop stand-in for the live audience: key 1 / button 1 = NORMAL, key 2 / button 2 = STRANGE.
    /// Counts votes until the timer runs out; the first key press ends the vote immediately when
    /// endOnFirstVote is true (default, handy while testing).
    /// </summary>
    public class KeyboardVoteProvider : MonoBehaviour, IVoteProvider
    {
        [Header("UI (built by GreyboxBuilder)")]
        public GameObject panel;
        public TMP_Text titleText;
        public TMP_Text timerText;
        public Button normalButton;
        public Button strangeButton;
        public TMP_Text normalLabel;
        public TMP_Text strangeLabel;
        public TMP_Text tallyText;

        [Header("Behaviour")]
        public bool endOnFirstVote = true;

        Action<bool> _onResult;
        Coroutine _routine;
        int _normalVotes, _strangeVotes;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (normalButton != null) normalButton.onClick.AddListener(() => CastVote(false));
            if (strangeButton != null) strangeButton.onClick.AddListener(() => CastVote(true));
        }

        public void StartVote(ForkDefinition fork, Action<bool> onResult)
        {
            CancelVote();
            _onResult = onResult;
            _normalVotes = _strangeVotes = 0;
            if (titleText != null) titleText.text = fork.title;
            if (normalLabel != null) normalLabel.text = $"1  {fork.normalLabel}";
            if (strangeLabel != null) strangeLabel.text = $"2  {fork.strangeLabel}";
            if (panel != null) panel.SetActive(true);
            UpdateTally();
            _routine = StartCoroutine(VoteRoutine(fork));
        }

        public void CancelVote()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            _onResult = null;
            if (panel != null) panel.SetActive(false);
        }

        /// <summary>Also callable from a future network provider or a UI button.</summary>
        public void CastVote(bool strange)
        {
            if (_onResult == null) return;
            if (strange) _strangeVotes++; else _normalVotes++;
            UpdateTally();
            if (endOnFirstVote) Finish(strange);
        }

        IEnumerator VoteRoutine(ForkDefinition fork)
        {
            float remaining = fork.voteSeconds;
            while (remaining > 0f)
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) CastVote(false);
                    else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) CastVote(true);
                }
                if (_onResult == null) yield break; // finished by CastVote
                remaining -= Time.unscaledDeltaTime;
                if (timerText != null) timerText.text = Mathf.CeilToInt(remaining).ToString();
                yield return null;
            }

            bool strangeWins;
            if (_strangeVotes == _normalVotes) strangeWins = fork.strangeWinsOnTimeout;
            else strangeWins = _strangeVotes > _normalVotes;
            Finish(strangeWins);
        }

        void Finish(bool strange)
        {
            var cb = _onResult;
            _onResult = null;
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            if (panel != null) panel.SetActive(false);
            cb?.Invoke(strange);
        }

        void UpdateTally()
        {
            if (tallyText != null) tallyText.text = $"normal {_normalVotes}   strange {_strangeVotes}";
        }
    }
}
