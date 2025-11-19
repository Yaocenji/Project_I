#pragma once

#define MY_PI 3.14159265358979323846
#define MY_TWO_PI 6.28318530717958647693

// 2D点光源数据
struct SpotLight2DData
{
    float4 position_intensity_falloff;
    float4 inoutRadius_inoutAngles;
    float4 color_direction;
};

// 阴影贴图 数据
struct ShadowMapInfo
{
    float Depth;
    uint Id;
};

// 有阴影点光源系列
StructuredBuffer<SpotLight2DData> SpotLight2D_Shadowed_Data_Buffer;
// 无阴影点光源系列
StructuredBuffer<SpotLight2DData> SpotLight2D_NoShadowed_Data_Buffer;

// 获取光源各项数据
float2 GetSpotLightPosition(inout SpotLight2DData light_data)
{
    return light_data.position_intensity_falloff.xy;
}
float GetSpotLightIntensity(inout SpotLight2DData light_data)
{
    return light_data.position_intensity_falloff.z;
}

// 此项目所用的衰减相关的函数集合
float PT(float x)
{
    return 0.5 / x;
}
// 输入：x∈[0,1]，t∈[0,1]，x接近0则为1，x接近1则为0，t越大衰减越快，反之越慢
float Atten(float x, float t)
{
    x = clamp(x, 0, 1);
    t = clamp(t, 0, 1);
    return t <= 0.5 ?
            pow(1 - pow(x, PT(t)), 2 * t) :
        1 - pow(1 - pow(1 - x, PT(1 - t)), 2 * (1 - t));
}
// 世界空间的点光源衰减计算：1、角度衰减；2、半径衰减；
// 使用：方向 衰减系数 四个inner outer数值
// 输出：0~1 的衰减系数
float GetSpotLightAttenuationWS(inout SpotLight2DData light_data, float2 light_2_frag)
{
    // fall off 作为t值，fall off越大t越大衰减越强
    float fallOff = clamp(light_data.position_intensity_falloff.w, 0, 1);
    
    float innerRadius = light_data.inoutRadius_inoutAngles.x;
    float outerRadius = light_data.inoutRadius_inoutAngles.y;
    float innerAngleRad = radians(light_data.inoutRadius_inoutAngles.z);
    float outerAngleRad = radians(light_data.inoutRadius_inoutAngles.w);

    // 首先进行半径衰减
    // 计算距离
    float dist = length(light_2_frag);
    // 相对半径的 01距离
    float relateDist = dist <= innerRadius ? 0 : dist >= outerRadius ? 1 : (dist - innerRadius) / (outerRadius - innerRadius);
    // 使用01距离计算01衰减
    float DistAtten = Atten(relateDist, fallOff);

    // 然后是角度衰减
    // 先计算该光源自身的中心方向
    float dirAngleRad = radians(light_data.color_direction.w);
    // 单位圆确定方向向量
    float2 centerDir = float2(cos(dirAngleRad), sin(dirAngleRad));
    // 计算当前光源到片元的方向
    float2 lightDir = normalize(light_2_frag);
    // 两个之间的夹角（弧度制，不大于π）
    float incRadian = acos(dot(lightDir, centerDir));
    // 相对光源角参数的 01 角
    float relateRad = incRadian <= innerAngleRad ? 0 : incRadian >= outerAngleRad ? 1 : (incRadian - innerAngleRad) / (outerAngleRad - innerAngleRad);
    // 使用01距离衰减计算01衰减
    float RadianAtten = Atten(relateRad, fallOff);

    // 返回积
    return DistAtten * RadianAtten;
}

float3 GetSpotLightColor(inout SpotLight2DData light_data)
{
    return light_data.color_direction.xyz;
}



// 有阴影点光源的阴影（128*2048的数据：对于至多128个光源，将360°分割为2048份数，记录每份对应的距离）
StructuredBuffer<ShadowMapInfo> SpotLight2D_ShadowMap_Buffer;

// 根据光源index和方位向量计算阴影的index
int2 GetSpotLightShadowBufferIndex(int spotLightIndex, float2 light_2_frag)
{
    // 计算对应的行偏移：
    int xOffset = spotLightIndex;

    // 计算对应的列偏移：
    SpotLight2DData lightData = SpotLight2D_Shadowed_Data_Buffer[spotLightIndex];
    float2 directionRange = float2(lightData.color_direction.w - lightData.inoutRadius_inoutAngles.w,
                    lightData.color_direction.w + lightData.inoutRadius_inoutAngles.w);
    // 先算light_2_frag对应的方位角
    light_2_frag = normalize(light_2_frag);
    // 这是方位角（弧度制）
    float radians = light_2_frag.y >= 0 ? acos(light_2_frag.x) : MY_TWO_PI - acos(light_2_frag.x);
    radians = clamp(radians, 0, MY_TWO_PI);
    
    if (radians + MY_TWO_PI >= directionRange.x && radians + MY_TWO_PI <= directionRange.y )
    {
        radians += MY_TWO_PI;
    }
    if (radians - MY_TWO_PI >= directionRange.x && radians - MY_TWO_PI <= directionRange.y )
    {
        radians -= MY_TWO_PI;
    }
    
    // 根据方位角，计算2048列中的哪一列（0~2π映射到0~2048）
    int yOffset = (radians >= directionRange.x && radians <= directionRange.y ) ?
        round((radians - directionRange.x) / (lightData.inoutRadius_inoutAngles.w * 2) * 2048) : -1;

    // 最终结果是相加：
    return int2(xOffset, yOffset);
}

// 将世界空间距离映射为01定点数距离
uint GetSpotLightDistanceWorld2SpotLight01(inout SpotLight2DData light_data, float distWS)
{
    // 该光源的最大距离
    float maxRadius = light_data.inoutRadius_inoutAngles.y;
    // 01最大距离
    float dist01 = distWS >= maxRadius ? 1 : distWS / maxRadius;
    // 将01最大距离乘上uint最大值，就是定点表示的01最大值了
    return int(dist01 * 0xFFFFFFFF);
}
