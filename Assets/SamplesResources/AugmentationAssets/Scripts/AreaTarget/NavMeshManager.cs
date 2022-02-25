/*========================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/

using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    [Header("Area Target Position")]
    public Transform AreaTargetTransform;
    
    [Header("Navigation Agent and its ARCamera")]
    public Transform ArCameraTransform;
    public NavMeshAgent NavigationAgent;
    
    [Header("Navigation Line")]
    public LineRenderer NavigationLine;
    
    Vector3 mAreaTargetOriginalPosition;
    Vector3 mCurrentDestination;
    bool mIsNavigating;

    const float DISTANCE_THRESHOLD = 1.5f;

    void Awake()
    {
        mAreaTargetOriginalPosition = AreaTargetTransform.transform.position;
    }

    void Update()
    {
        UpdateNavigationAgentPosition();
        IsNavLineVisible();
        UpdateNavigationLine();
    }

    void UpdateNavigationAgentPosition()
    {
        // updates the navigation Agent's position inside the NavMesh, based on the ARCamera's current position
        // in relation to the AreaTarget's current position
        var arCamPositionInAreaTarget = AreaTargetTransform.InverseTransformPoint(ArCameraTransform.position);
        NavigationAgent.transform.localPosition = arCamPositionInAreaTarget + mAreaTargetOriginalPosition;
    }
    
    void IsNavLineVisible()
    {
        if (Vector3.Distance(NavigationAgent.transform.position, mCurrentDestination) < DISTANCE_THRESHOLD)
        {
            mIsNavigating = false;   
            NavigationLine.enabled = false;
        }
    }

    void UpdateNavigationLine()
    {
        if (mIsNavigating && !NavigationAgent.pathPending)
        {
            DrawPath();
            NavigationAgent.SetDestination(mCurrentDestination);
        }
    }

    public void NavigateTo(Transform destinationTransform)
    {
        NavigationLine.enabled = true;
        mIsNavigating = true;
        // position which is from the Moving AreaTarget space has to be transformed into the Static Navmesh space
        var localPositionInAreaTarget = AreaTargetTransform.InverseTransformPoint(destinationTransform.position);
        mCurrentDestination = localPositionInAreaTarget + mAreaTargetOriginalPosition;
        NavigationAgent.SetDestination(mCurrentDestination);
    }

    void DrawPath()
    {
        NavigationLine.positionCount = NavigationAgent.path.corners.Length;
        // we have to transform the positions from the Static Navmesh space back to the Moving AreaTarget space
        var transformedNavigationCorners = new Vector3[NavigationLine.positionCount];
        for (int i = 0; i < transformedNavigationCorners.Length; i++)
            transformedNavigationCorners[i] = AreaTargetTransform.TransformPoint(NavigationAgent.path.corners[i] - mAreaTargetOriginalPosition);
        NavigationLine.SetPositions(transformedNavigationCorners);
    }
}
