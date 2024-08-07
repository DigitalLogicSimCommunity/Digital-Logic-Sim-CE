﻿using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Core.Simulation;
using UnityEngine;
using UnityEngine.Serialization;
using VitoBarra.System.Interaction;

public class Wire : Interactable
{
    //Event
    public event Action<List<Vector2>> OnWireChange;

    public event Action OnPlacing;

    public event Action<Wire> OnWireDestroy;


    bool wireConnected;

    [FormerlySerializedAs("startPin")] public Pin SourcePin;

    [FormerlySerializedAs("targetPin")] [FormerlySerializedAs("endPin")]
    public Pin TargetPin;


    public List<Vector2> anchorPoints { get; private set; }


    public Pin ChipInputPin => (SourcePin.pinType == Pin.PinType.ChipInput) ? SourcePin : TargetPin;
    public Pin ChipOutputPin => (SourcePin.pinType == Pin.PinType.ChipOutput) ? SourcePin : TargetPin;


    private void OnEnable()
    {
        Manager.ChipInteraction.onChipMovement += UpdateWirePos;
        ScalingManager.i.OnScaleChange += UpdateWirePos;
    }

    private void OnDestroy()
    {
        Manager.ChipInteraction.onChipMovement -= UpdateWirePos;
        ScalingManager.i.OnScaleChange -= UpdateWirePos;
    }

    public void SetAnchorPoints(Vector2[] newAnchorPoints)
    {
        anchorPoints = new List<Vector2>(newAnchorPoints);
        NotifyWireChange();
    }

    private void UpdateWirePos()
    {
        if (!wireConnected) return;

        const float maxSqrError = 0.00001f;
        // How far are start and end points from the pins they're connected to (chip
        // has been moved)
        Vector2 startPointError =
            (Vector2)SourcePin.transform.position - anchorPoints[0];
        Vector2 endPointError = (Vector2)TargetPin.transform.position -
                                anchorPoints[^1];

        if (!(startPointError.sqrMagnitude > maxSqrError) && !(endPointError.sqrMagnitude > maxSqrError)) return;

        // If start and end points are both same offset from where they should be,
        // can move all anchor points (entire wire)
        if ((startPointError - endPointError).sqrMagnitude < maxSqrError &&
            startPointError.sqrMagnitude > maxSqrError)
        {
            for (int i = 0; i < anchorPoints.Count; i++)
            {
                anchorPoints[i] += startPointError;
            }
        }

        anchorPoints[0] = SourcePin.transform.position;
        anchorPoints[^1] = TargetPin.transform.position;
        NotifyWireChange();
    }


    public void Connect(Pin inputPin, Pin outputPin)
    {
        ConnectToFirstPin(inputPin);
        Place(outputPin);
        UpdateWirePos();
    }

    public void ConnectToFirstPin(Pin startPin)
    {
        this.SourcePin = startPin;


        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

        anchorPoints = new List<Vector2>();

        anchorPoints.Add(startPin.transform.position);
        anchorPoints.Add(startPin.transform.position);

        NotifyWireChange();
    }

    public void ConnectToFirstPinViaWire(Pin startPin, Wire parentWire, Vector2 inputPoint)
    {
        anchorPoints = new List<Vector2>();


        this.SourcePin = startPin;
        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);


        // Find point on wire nearest to input point
        Vector2 closestPoint = Vector2.zero;
        float smallestDst = float.MaxValue;
        int closestI = 0;
        for (int i = 0; i < parentWire.anchorPoints.Count - 1; i++)
        {
            var a = parentWire.anchorPoints[i];
            var b = parentWire.anchorPoints[i + 1];
            var pointOnWire = MathUtility.ClosestPointOnLineSegment(a, b, inputPoint);
            float sqrDst = (pointOnWire - inputPoint).sqrMagnitude;
            if (sqrDst < smallestDst)
            {
                smallestDst = sqrDst;
                closestPoint = pointOnWire;
                closestI = i;
            }
        }

        for (int i = 0; i <= closestI; i++)
        {
            anchorPoints.Add(parentWire.anchorPoints[i]);
        }

        anchorPoints.Add(closestPoint);
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            anchorPoints.Add(closestPoint);
        }

        anchorPoints.Add(inputPoint);

        NotifyWireChange();
    }

    // Connect the input pin to the output pin
    public void Place(Pin endPin)
    {
        this.TargetPin = endPin;
        anchorPoints[^1] = endPin.transform.position;

        wireConnected = true;

        OnPlacing?.Invoke();
        NotifyWireChange();

        if (endPin.pinType == Pin.PinType.ChipOutput)
            SwapStartEndPoints();
    }

    public void DestroyWire()
    {
        Pin.RemoveConnection(SourcePin, TargetPin);
        TargetPin.ReceiveZero();
        Destroy(gameObject);
        OnWireDestroy?.Invoke(this);
    }

    void SwapStartEndPoints()
    {
        (SourcePin, TargetPin) = (TargetPin, SourcePin);
        anchorPoints.Reverse();
        NotifyWireChange();
    }


    void NotifyWireChange()
    {
        OnWireChange?.Invoke(anchorPoints);
    }

    // Update position of wire end point (for when initially placing the wire)
    public void UpdateWireEndPoint(Vector2 endPointWorldSpace)
    {
        anchorPoints[^1] = ProcessPoint(endPointWorldSpace);
        NotifyWireChange();
    }

    // Add anchor point (for when initially placing the wire)
    public void AddAnchorPoint(Vector2 pointWorldSpace)
    {
        anchorPoints[^1] = ProcessPoint(pointWorldSpace);
        anchorPoints.Add(ProcessPoint(pointWorldSpace));
        NotifyWireChange();
    }


    Vector2 ProcessPoint(Vector2 endPointWorldSpace)
    {
        if (!Input.GetKey(KeyCode.LeftShift)) return endPointWorldSpace;
        Vector2 a = anchorPoints[^2];
        Vector2 b = endPointWorldSpace;

        bool xAxisLonger = (Mathf.Abs(a.x - b.x) > Mathf.Abs(a.y - b.y));
        return xAxisLonger ? new Vector2(b.x, a.y) : new Vector2(a.x, b.y);
    }


    public override void OrderedUpdate()
    {
    }

    public override void DeleteCommand()
    {
        DestroyWire();
    }
}