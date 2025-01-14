using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Cohort.Games.Planes
{
    public class Countdown : MonoBehaviour
    {
        public Action onCountdownFinished;

        //duration of one number
        [SerializeField] private float _stepDur;
        [SerializeField] private int _steps;
        [SerializeField] private TMP_Text _oddNum, _evenNum;

        [SerializeField] private CountDownAnimationProps _animProps = CountDownAnimationProps.Fade | CountDownAnimationProps.ScaleUp;
        
        private bool isEven;

        public static Countdown CreateCountdownComponent(GameObject obj, TMP_Text odd, TMP_Text even, 
                                                         float duration, int speed = 1, CountDownAnimationProps animProps = (CountDownAnimationProps.Fade | CountDownAnimationProps.ScaleUp))  {
            Countdown comp = obj.AddComponent<Countdown>();

            comp._oddNum = odd;
            comp._evenNum = even;
            comp._steps =  Mathf.RoundToInt(duration * speed);
            comp._stepDur = duration / comp._steps;
            comp._animProps = animProps;
            
            return comp;
        }

        private void Awake()
        {
            SetInactive(_oddNum);
            SetInactive(_evenNum);
        }

        public void StartCountdown()
        {
            isEven = _steps % 2 == 0;

            StartCoroutine(Count());
        }

        private IEnumerator Count()
        {
            int step = _steps;
            while (step > 0) {
                if (isEven) {
                    _evenNum.gameObject.SetActive(true);
                    _evenNum.text = step.ToString();
                    
                    DoAnimateProperties(_evenNum);

                    SetInactive(_oddNum);
                }
                else {
                    _oddNum.gameObject.SetActive(true);
                    _oddNum.text = step.ToString();
                    
                    DoAnimateProperties(_oddNum);
                    
                    SetInactive(_evenNum);
                }

                yield return new WaitForSeconds(_stepDur);
                isEven = !isEven;
                step--;
            }

            OnCountdownFinished();
        }

        private void DoAnimateProperties(TMP_Text field) {
            if (_animProps.HasFlag(CountDownAnimationProps.ScaleUp)) {
                field.transform.DOScale(Vector3.one, _stepDur);
            }

            if (_animProps.HasFlag(CountDownAnimationProps.Fade)) {
                field.DOFade(0f, _stepDur);
            }
        }

        private void SetInactive(TMP_Text field)
        {
            field.gameObject.SetActive(false);
            if (_animProps.HasFlag(CountDownAnimationProps.ScaleUp)) {
                field.transform.localScale = Vector3.zero;
            }
            
            Color c = field.color;
            c.a = 1f;
            field.color = c;
        }

        private void OnCountdownFinished()
        {
            onCountdownFinished?.Invoke();
            
            SetInactive(_oddNum);
            SetInactive(_evenNum);
        }
        
        [Flags]
        public enum CountDownAnimationProps
        {
            None = 0,
            Fade = 1<<0,
            ScaleUp = 1<<1,
        }
    }
}