using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("移動與跳躍參數")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 22.0f; // 可在 Inspector 依手感調整

    [Header("終點設定")]
    [Tooltip("角色 X 座標小於此數值時視為抵達終點 (左側約 -5.0)")]
    [SerializeField] private float destinationX = -5.0f;

    // 作業要求：務必使用 Vector 與 陣列 (Array)
    private Vector2[] moveVectors;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = false;
    private float originalScaleX;
    private string currentAnimation = "";

    // 是否抵達終點
    private bool hasReachedDestination = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        originalScaleX = Mathf.Abs(transform.localScale.x);

        // 作業要求：Vector 陣列初始化
        moveVectors = new Vector2[4]
        {
            Vector2.zero,   // [0] 靜止
            Vector2.left,   // [1] 向左 (A 鍵)
            Vector2.right,  // [2] 向右 (D 鍵)
            Vector2.up      // [3] 向上跳躍 (空白鍵)
        };

        ChangeAnimation("Idle_01");
    }

    void Update()
    {
        HandleMovement();
        CheckDestination();
    }

    void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        int horizontalIndex = 0;
        bool isWalking = false;

        // 1. 水平移動與轉向
        if (keyboard.aKey.isPressed)
        {
            horizontalIndex = 1;
            isWalking = true;
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if (keyboard.dKey.isPressed)
        {
            horizontalIndex = 2;
            isWalking = true;
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
        }

        // 使用 Vector 陣列賦值
        Vector2 horizontalVelocity = moveVectors[horizontalIndex] * moveSpeed;
        rb.linearVelocity = new Vector2(horizontalVelocity.x, rb.linearVelocity.y);

        // 2. 空白鍵跳躍
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(moveVectors[3] * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            ChangeAnimation("Jump_01");
        }

        // 3. 動畫狀態切換
        if (isGrounded)
        {
            if (isWalking)
                ChangeAnimation("Walk_01");
            else
                ChangeAnimation("Idle_01");
        }
    }

    // 檢查是否抵達終點
    void CheckDestination()
    {
        // 當角色走到左側目標座標，且在地面上時
        if (!hasReachedDestination && transform.position.x <= destinationX && isGrounded)
        {
            hasReachedDestination = true;
            Debug.Log("<color=green>【恭喜】已順利跨越障礙，抵達終點！</color>");
        }
    }

    // 在畫面上直接繪製「抵達終點」提示文字 (不需要手動架設 UI Canvas)
    void OnGUI()
    {
        if (hasReachedDestination)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 42;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;

            // 在畫面正上方顯示大字
            GUI.Label(new Rect(Screen.width / 2 - 200, 80, 400, 80), "🎉 抵達終點！ 🎉", style);
        }
    }

    void ChangeAnimation(string triggerName)
    {
        if (anim == null || currentAnimation == triggerName) return;
        anim.SetTrigger(triggerName);
        currentAnimation = triggerName;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}