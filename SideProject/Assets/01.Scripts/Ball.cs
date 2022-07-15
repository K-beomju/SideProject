using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    #region variable
    [SerializeField] private GameObject arrow, ballPv;
    [SerializeField] private LineRenderer ballLR;

    private Vector3 firstPos;
    private Vector3 mousePos, gap;

    private Camera mainCam;
    private Rigidbody2D rb;

    private bool isFire;
    private bool isMove;
    #endregion

    #region Message
    private void Awake()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (CompareTag("Ball")) StartCoroutine(OnCollsionEnter2D_Ball(col));
    }

    private void FixedUpdate()
    {
        if (isFire)
        {
            Launch(gap);
            isFire = false;
        }
    }

    private void Update()
    {
        bool isMouse = Input.GetMouseButton(0);
        if (isMouse)
        {
            mousePos = Utills.MousePos;
            if (mousePos.magnitude < 1) return;

            gap = (mousePos - transform.position).normalized;
            gap = new Vector3(gap.y >= 0 ? gap.x : gap.x >= 0 ? 1 : -1, Mathf.Clamp(gap.y, 0.2f, 1), 0); // 각도 제한 

            arrow.transform.position = firstPos; // 처음 시작 지점 
            arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(gap.y, gap.x) * Mathf.Rad2Deg); // 화살표 마우스 위치에 따른 각도 회전


            // 벽 끝쪽에 프리뷰 볼 그려주기 
            ballPv.transform.position =
                Physics2D.CircleCast(new Vector2(Mathf.Clamp(firstPos.x, -54, 54), GameManager.groundY), 1.8f, gap, Mathf.Infinity,
                1 << LayerMask.NameToLayer("Wall") | 1 << LayerMask.NameToLayer("Success")).centroid;

            RaycastHit2D hit = Physics2D.Raycast(firstPos, gap, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall"));

            ballLR.SetPosition(0, firstPos);
            ballLR.SetPosition(1, (Vector3)hit.point - gap * 1.5f);
        }
        arrow.SetActive(isMouse);
        ballPv.SetActive(isMouse);

        if (Input.GetMouseButtonUp(0))
        {
            if (mousePos.magnitude < 1) return;
            ballLR.SetPosition(0, Vector3.zero);
            ballLR.SetPosition(1, Vector3.zero);

            isFire = true;

            firstPos = Vector3.zero;
            mousePos = Vector3.zero;
        }
    }
    #endregion



    private IEnumerator OnCollsionEnter2D_Ball(Collision2D col)
    {
        GameObject ground = col.gameObject;
        Physics2D.IgnoreLayerCollision(2,2); // 공끼리는 충돌 안나게

        if (ground.CompareTag("Ground"))
        { 
            rb.velocity = Vector2.zero;
            transform.position  = new Vector2(col.contacts[0].point.x , GameManager.groundY);
            VeryFirstPosSet(transform.position);

            while(true)
            {
                yield return null;
                transform.position = Vector3.MoveTowards(transform.position , firstPos, 4);
                if(transform.position == firstPos) { isMove = false; yield break; }
            }
        }
    }

    private void Launch(Vector3 pos)
    {
        //shotTrigger = true;
        isMove = true;
        rb.AddForce(pos * 7000);
    }

    private void VeryFirstPosSet(Vector3 pos)
    {
        if (firstPos == Vector3.zero) firstPos = pos;
    }

}
