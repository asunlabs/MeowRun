using System.Transactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    //애니메이터 컴포넌트의 레퍼런스 가져와 저장
    private Animator animator;

    private Transform _transform;
    private Rigidbody playerRigidbody;
    public bool _isJumping;
    //오브젝트의 초기 높이
    private float _posY;
    //중력가속도
    private float _gravity;
    //점프력
    private float _jumpPower;
    //점프 이후 경과시간
    private float _jumpTime;
    [SerializeField] float speed = 3f; // 유니티 에디터에서 스피드 변수 조정 가능

    //키보드 조작 가능 여부
    private bool canMove = true;
    //플레이어 좌표값 고정 여부
    private bool stopPosition = false;
    //플레이어 좌표를 고정하기 위한 위치값
    private Vector3 temp;

    void Start()
    {
        print("Game started"); // UnityEngine.Debug.Log => print(same but shorter)

        animator = GetComponent<Animator>();
        playerRigidbody = this.GetComponent<Rigidbody>(); // 게임 시작 시 캐릭터 선택

        _transform = transform;
        _isJumping = false;
        _posY = transform.position.y;
        _gravity = 9.8f;
        _jumpPower = 5.0f;
        _jumpTime = 0.0f;
    }

    void Update()
    {
        //캐릭터 회전값 고정(뒤집어지지 않게)
        //
        transform.eulerAngles = new Vector3(transform.rotation.x, 90.0f, transform.rotation.z);

        if (canMove)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !_isJumping)
            {
                animator.SetBool("playerJump", true);
                _isJumping = true;
                _posY = _transform.position.y;
            }

            if (_isJumping)
            {
                Jump(); // To do : 점프 사운드 추가할 것
            }

            // FIX : 키보드 상/하/좌/우 A, S, D, W. 3D 맵상에서 상하좌우로 움직일 수 있어야 한다고 판단, 
            // 기존 좌/우 움직임 제한 => 상/하/좌/우 확대
            moveCharacter();
        }

        if(stopPosition)
            this.transform.position = temp;
    }


    // ================= 플레이어 이동 로직 ================= //
    // 키보드 세팅
    void moveCharacter()
    {
        string INPUT_HORIZONTAL = "Horizontal";
        string INPUT_VERTICAL = "Vertical";
        float horizontalInput = Input.GetAxis(INPUT_HORIZONTAL);
        float verticalInput = Input.GetAxis(INPUT_VERTICAL);
        playerRigidbody.velocity = new Vector3(horizontalInput * speed, playerRigidbody.velocity.y, verticalInput * speed);
    }

    // 점프 기능
    void Jump()
    {
        //y=-a*x+b에서 (a: 중력가속도, b: 초기 점프속도)
        //적분하여 y = (-a/2)*x*x + (b*x) 공식을 얻는다.(x: 점프시간, y: 오브젝트의 높이)
        //변화된 높이 height를 기존 높이 _posY에 더한다.
        float height = (_jumpTime * _jumpTime * (-_gravity) / 2) + (_jumpTime * _jumpPower);
        _transform.position = new Vector3(_transform.position.x, _posY + height, _transform.position.z);
        //점프시간을 증가시킨다.
        _jumpTime += Time.deltaTime;

        //처음의 높이 보다 더 내려 갔을때 => 점프전 상태로 복귀한다.
        if (height < 0.0f)
        {
            animator.SetBool("playerJump", false);
            _isJumping = false;
            _jumpTime = 0.0f;
            _transform.position = new Vector3(_transform.position.x, _posY, _transform.position.z);
        }
    }
    // ================= 플레이어 이동 로직 ================= //


    // TO DO : 플레이어 이동 로직 <=> 리스폰 로직 서로 다른 스크립트로 분리하기. 
    // ================= 플레이어 리스폰 로직 ================= //
    // 장애물 충돌시 리스폰
    void OnCollisionEnter(Collision other)
    {
        print("on collision executed");
        // 오브젝트 태그가 'Enemies'일 경우 scene 리로드
        if (other.gameObject.CompareTag("Enemies"))
        {
            //닿았을 때의 위치값 저장
            temp = transform.position;
            StartCoroutine(playerDieCoroutine());
        }
    }

    IEnumerator playerDieCoroutine()
    {
        stopPosition = true;
        canMove = false;
        this.GetComponent<BoxCollider>().enabled = false;
        playerRigidbody.useGravity = false;

        //플레이어가 쓰러지는 애니메이션 재생
        animator.SetBool("playerDie", true);

        //1초 대기
        yield return new WaitForSeconds(1f);

        //플레이어 오브젝트가 재생성되면서 초기값인 false로 바뀌므로 변경 불필요
        //stopPosition = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    // To do : 인공지능 적군 추가할 것.
    // ================= 플레이어 리스폰 로직 ================= //
}
