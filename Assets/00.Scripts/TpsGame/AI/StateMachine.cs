using UnityEngine;

public class StateMachine : MonoBehaviour
{
    State currentState;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState!=null)
            currentState.Perform();
    }

    public void ChangeState(State newState)
    {
        //check activeState 
        if (currentState != null)
        {
            //이전 스테이트 정리
            currentState.Exit();
        }
        //새로운 스테이트 진입
        currentState = newState;

        //페일 세이프:제대로 스테이트가 들어갔는지 확인
        if (currentState != null)
        {
            //새 스테이트가 자리잡게 세팅
            currentState.machine = this;
            
            currentState.Enter();
        }
    }
}
