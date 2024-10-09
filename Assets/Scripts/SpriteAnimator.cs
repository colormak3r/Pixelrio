using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SpriteAnimation
{
    public string Name;
    public Sprite[] Sprites;
    public bool canBeInterupted;

    public int FrameCount => Sprites == null ? 0 : Sprites.Length;
}


public class SpriteAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private string commonName;
    [SerializeField]
    private float frameDuration = 0.1f;
    [SerializeField]
    private List<SpriteAnimation> animations = new List<SpriteAnimation>();

    [Header("Debugs")]
    [SerializeField]
    private SpriteAnimation currentAnimation;

    private SpriteRenderer spriteRenderer;

    private Dictionary<string, SpriteAnimation> animationsDictionary = new Dictionary<string, SpriteAnimation>();
    private float nextFrame;
    private int frameIndex;
    private int frameCount;

    private SpriteAnimation queueAnimation;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animations.Count == 0 && commonName != null)
        {
            animations = SpriteManager.Main.GetReserve(commonName);
        }

        //Debug.Log(gameObject.name + " animation count = " + animations.Count);

        foreach (var animation in animations)
            animationsDictionary[animation.Name] = animation;

        if (animations.Count > 0)
        {
            if (animationsDictionary.ContainsKey("Idle"))
                ChangeAnimation("Idle");
            else
                ChangeAnimation(animations[0].Name);
        }
    }

    private void Update()
    {
        if (currentAnimation.FrameCount == 0) return;

        if (Time.time > nextFrame)
        {
            nextFrame = Time.time + frameDuration;
            spriteRenderer.sprite = currentAnimation.Sprites[frameIndex];

            frameIndex++;
            if (frameIndex >= frameCount)
            {
                frameIndex = 0;
                if (queueAnimation.FrameCount > 0)
                {
                    //Debug.Log($"{currentAnimation} completed, change to {queueAnimation.Name}");
                    currentAnimation = queueAnimation;
                    frameCount = currentAnimation.FrameCount;
                }
            }

        }
    }

    public void AddAnimation(SpriteAnimation newAnimation)
    {
        animations.Add(newAnimation);
        animationsDictionary[newAnimation.Name] = newAnimation;
    }

    public void ChangeAnimation(string animationName)
    {
        if (!animationsDictionary.ContainsKey(animationName))
        {
            Debug.Log($"{gameObject.name} does not contains animation named {animationName}");
            return;
        }
        else
        {
            if (currentAnimation.Name == animationName) return;

            if (currentAnimation.FrameCount > 0 && currentAnimation.canBeInterupted)
            {
                //Debug.Log($"{animationName} trying to interupt {currentAnimation.Name}");
                queueAnimation = animationsDictionary[animationName];
                return;
            }

            //Debug.Log($"Change to {currentAnimation.Name}");

            currentAnimation = animationsDictionary[animationName];
            frameIndex = 0;
            frameCount = currentAnimation.FrameCount;
        }
    }

    public void LoadAnimation(List<SpriteAnimation> newAnimations)
    {
        animations = newAnimations;
    }

    [ContextMenu("Mock Animation Change")]
    private void MockAnimationChange()
    {
        ChangeAnimation(animations[0].Name);
    }
}
