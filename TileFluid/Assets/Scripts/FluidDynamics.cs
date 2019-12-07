using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidDynamics : MonoBehaviour {

    private WorldGenerator world;
    public GameObject tileBlockPrefap;

    public bool useNoise = false;
    public void SetUsingNoise(bool value) { useNoise = value; }

    public float fluidUpdateRate = 0.08f;
    public void setUpdateSpeed(float value)
    {
        fluidUpdateRate += value;
        if (fluidUpdateRate < 0.02f) fluidUpdateRate = 0.02f;
    }
    public float maxAmount = 1.0f;
    public float minAmount = 0.02f;
    public float viscosity = 0.5f;
    
    private TileBlock[,] tileBlocks;
    private float[,] fluidDifference;

    private bool isGenerating = false;
    public void Generate()
    {
        isGenerating = true;
        
        // 블록 생성 
        for (int x = 0; x < world.width; x++)
        {
            for (int y = 0; y < world.height; y++)
            {
                if (useNoise)
                {
                    float noise = CalcNoise(x, y);
                    tileBlocks[x, y].SetTileStatusByNoise(noise);
                }
                else tileBlocks[x, y].SetTileStatusByRandom();
            }
        }

        WorldGenerator.Instance.ResetCameraPos();

        isGenerating = false;
    }

    private void Awake()
    {
        world = WorldGenerator.Instance;

        tileBlocks = new TileBlock[world.width, world.height];
        fluidDifference = new float[world.width, world.height];

        // 블록 생성 
        for (int x = 0; x < world.width; x++)
        {
            for (int y = 0; y < world.height; y++)
            {
                tileBlocks[x, y] = Instantiate(tileBlockPrefap, new Vector3(x, y, 0), Quaternion.identity, transform).GetComponent<TileBlock>();

                if(useNoise)
                {
                    float noise = CalcNoise(x, y);
                    tileBlocks[x, y].SetTileStatusByNoise(noise);
                }
                else tileBlocks[x, y].SetTileStatusByRandom();
            }
        }
        
        // 인접 블록 저장 
        for (int x = 0; x < world.width; x++)
        {
            for (int y = 0; y < world.height; y++)
            {
                tileBlocks[x, y].topBlock = GetTileBlock(x, y + 1);
                tileBlocks[x, y].bottomBlock = GetTileBlock(x, y - 1);
                tileBlocks[x, y].leftBlock = GetTileBlock(x - 1, y);
                tileBlocks[x, y].rightBlock = GetTileBlock(x + 1, y);
            }
        }
    }

    
    private void Start()
    {
        StartCoroutine(UpdateFluid());
    }

    IEnumerator UpdateFluid()
    {
        while(!isGenerating)
        {
            // start와 end 로 시뮬레이트할 블록들만 선택해서 비용 절감
            Vector2 startPosition = WorldGenerator.Instance.StartPos;
            Vector2 endPosition = WorldGenerator.Instance.EndPos;
            
            SimulateFluid(startPosition , endPosition);

            yield return new WaitForSeconds(fluidUpdateRate);
        }
    }
    
    void SimulateFluid(Vector2 startPosition, Vector2 endPosition)
    {
        int startX = (int)startPosition.x;
        int startY = (int)startPosition.y;
        int endX = (int)endPosition.x;
        int endY = (int)endPosition.y;
        
        // 이동할 물의 양 계산
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                TileBlock tileObj = GetTileBlock(x, y);
                if (tileObj.curAmount > minAmount)
                {
                    CalcFlowDifference(x, y, tileObj);
                }
            }
        }
        
        // 계산한 변화량 적용
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                TileBlock tileBlock = tileBlocks[x, y];

                // 벽의 경우 변화량이 없을거라 스킵
                if (tileBlock.status == TileStatus.DIRT || fluidDifference[x, y] == 0)
                    continue;
                
                // 해당 위치에 변화량 적용하고 조건에 따라 물, 빈칸으로 바꿔줌
                tileBlock.curAmount += fluidDifference[x, y];
                if (tileBlock.curAmount > minAmount) tileBlock.SetWater();
                else tileBlock.SetEmpty();
                
                fluidDifference[x, y] = 0;
            }
        }

    }

    void CalcFlowDifference(int x, int y, TileBlock tileBlock)
    {
        float flowAmount = 0.0f;
        float startAmount = tileBlock.curAmount;
        float remainingAmount = startAmount;
   
        // 아래 블록
        if (tileBlock.bottomBlock != null && (tileBlock.bottomBlock.status == TileStatus.WATER || tileBlock.bottomBlock.status == TileStatus.EMPTY))
        {
            // 현재 블록 + 아래 블록 한다음 이게 max값보다 크면 이동할 유체량을 조절 
            float combinedAmount = startAmount + tileBlock.bottomBlock.curAmount;
          
            if (combinedAmount <= maxAmount) flowAmount =  startAmount;
            else flowAmount = maxAmount - tileBlock.bottomBlock.curAmount;
            
            // 방어 코드 
            if (flowAmount < 0) flowAmount = 0;
            if (flowAmount > startAmount) flowAmount = startAmount;
            
            if (flowAmount > 0)
            {
                // 흐르고 남은 유체량
                remainingAmount -= flowAmount;

                // 유체 변화량 적용
                fluidDifference[x, y] -= flowAmount;
                fluidDifference[x, y - 1] += flowAmount;
                
                // 잔량이 별로 없으면 리턴
                if (remainingAmount < minAmount)
                    return;
            }
        }

        // 좌우측으로 흘러야 하는지 
        bool flowRight = tileBlock.rightBlock != null && 
            (tileBlock.rightBlock.status == TileStatus.WATER || tileBlock.rightBlock.status == TileStatus.EMPTY) && 
            remainingAmount > tileBlock.rightBlock.curAmount;

        bool flowLeft = tileBlock.leftBlock != null && 
            (tileBlock.leftBlock.status == TileStatus.WATER || tileBlock.leftBlock.status == TileStatus.EMPTY) &&
            remainingAmount > tileBlock.leftBlock.curAmount;

        // 양쪽으로 흘러야 할때
        if(flowRight && flowLeft)
        {
            // 3.0f 는 대충 감으로 
            float rightFlowAmount = (remainingAmount - tileBlock.rightBlock.curAmount) / 3.0f;

            if (rightFlowAmount < 0) rightFlowAmount = 0;
            if (rightFlowAmount > remainingAmount) rightFlowAmount = remainingAmount;

            float leftFlowAmount = (remainingAmount - tileBlock.leftBlock.curAmount) / 3.0f;

            if (leftFlowAmount < 0) leftFlowAmount = 0;
            if (leftFlowAmount > remainingAmount) leftFlowAmount = remainingAmount;
            
            if (rightFlowAmount > 0 )
            {
                remainingAmount -= rightFlowAmount;
                fluidDifference[x, y] -= rightFlowAmount;
                fluidDifference[x + 1, y] += rightFlowAmount;
                
                if (remainingAmount < minAmount)
                    return;
            }
            
            if (leftFlowAmount > 0)
            {
                remainingAmount -= leftFlowAmount;
                fluidDifference[x, y] -= leftFlowAmount;
                fluidDifference[x - 1, y] += leftFlowAmount;

                if (remainingAmount < minAmount)
                    return;
            }
        }
        else if (flowRight)
        {
            flowAmount = (remainingAmount - tileBlock.rightBlock.curAmount) / 2.0f;

            if (flowAmount < 0) flowAmount = 0;
            if (flowAmount > remainingAmount) flowAmount = remainingAmount;

            if (flowAmount > 0)
            {
                remainingAmount -= flowAmount;
                fluidDifference[x, y] -= flowAmount;
                fluidDifference[x + 1, y] += flowAmount;
                
                if (remainingAmount < minAmount)
                    return;
            }
        }
        else if (flowLeft)
        {
            flowAmount = (remainingAmount - tileBlock.leftBlock.curAmount) / 2.0f;
            if (flowAmount < 0) flowAmount = 0;
            if (flowAmount > remainingAmount) flowAmount = remainingAmount;

            if (flowAmount > 0)
            {
                remainingAmount -= flowAmount;
                fluidDifference[x, y] -= flowAmount;
                fluidDifference[x - 1, y] += flowAmount;

                if (remainingAmount < minAmount)
                    return;
            }
        }
    }

    public TileBlock GetTileBlock(int x, int y)
    {
        if (x < 0 || x >= world.width || y < 0 || y >= world.height)
            return null;

        return tileBlocks[x, y];
    }

    float CalcNoise(int x, int y)
    {
        // 노이즈 보정값
        float randomSeed = Random.Range(0.00f, 10000.0f);

        float xRatio = (float)x / (float)world.width;
        float yRatio = (float)y / (float)world.height;

        float xCond = Mathf.Sin(xRatio * Mathf.PI) * 0.75f + 0.25f;
        float yCond = (1 - yRatio) * 1.5f;

        float a = xCond * yCond;
        return Mathf.PerlinNoise(xRatio * randomSeed, yRatio * randomSeed) * a;
    }
}
