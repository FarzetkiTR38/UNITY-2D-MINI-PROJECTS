// The backdrop stays screen-centered while the iris can close around any input position.
// All artwork, placement and palette controls belong to the authored transition material.
sampler2D _BrandTex;
float4 _BrandTex_TexelSize, _BrandRect, _BrandCenter;
float _BrandWidth, _BrandOpacity, _BrandFeather, _BackdropGlow, _OrbitOpacity;

float PathStroke(float2 p, float2 a, float2 b)
{
    float2 delta=b-a;
    return length(p-a-delta*saturate(dot(p-a,delta)/dot(delta,delta)))-0.035;
}

fixed3 Background(float2 screenUV, float aspect)
{
    float2 center=float2(aspect*_BrandCenter.x,_BrandCenter.y);
    float2 fromCenter=screenUV-center;
    float brandAspect=(_BrandRect.z-_BrandRect.x)/max(_BrandRect.w-_BrandRect.y,0.001)*
        _BrandTex_TexelSize.z/max(_BrandTex_TexelSize.w,1);
    float brandWidth=min(_BrandWidth*aspect,0.68*brandAspect);
    float2 size=max(float2(brandWidth,brandWidth/max(brandAspect,0.001)),0.001);
    float halo=exp2(-dot(fromCenter/size,fromCenter/size)*2);
    fixed3 color=lerp(_Color.rgb,_BorderColor.rgb,halo*_BackdropGlow);

    // Thin puzzle-path motifs replace the old chunky arrow/star wallpaper.
    float2 tiled=(screenUV+float2(0.017,0.031))/max(_PatternSpacing,0.001);
    float2 cell=floor(tiled);
    float orientation=Hash(cell+31.3);
    float2 p=(frac(tiled)-0.5)/max(_PatternSize,0.01);
    if(orientation>0.5)p=p.yx;
    p.x*=orientation>0.75?-1:1;p.y*=orientation>0.25?-1:1;
    float arrow=min(PathStroke(p,float2(-0.18,-0.24),float2(-0.18,0.12)),
        PathStroke(p,float2(-0.18,0.12),float2(0.22,0.12)));
    arrow=min(arrow,min(PathStroke(p,float2(0.08,0.27),float2(0.23,0.12)),
        PathStroke(p,float2(0.23,0.12),float2(0.08,-0.03))));
    float sparkle=sqrt(abs(p.x))+sqrt(abs(p.y))-0.55;
    float isStar=step(0.8,Hash(cell+19.7));
    float shape=lerp(arrow,sparkle,isStar);
    float aa=max(fwidth(shape),0.003);
    float motif=(1-smoothstep(-aa,aa,shape))*step(Hash(cell),_PatternDensity);
    fixed4 ink=lerp(_PatternColor,_StarColor,isStar);
    color=lerp(color,ink.rgb,motif*_PatternOpacity*ink.a*(1-halo*0.65));

    // Restrained cyan and gold orbital arcs frame the existing game's illustrated identity.
    float radius=length(fromCenter*float2(1,1.12));
    float orbitRadius=brandWidth*0.62;
    float edge=min(abs(radius-orbitRadius),abs(radius-orbitRadius*1.17));
    float orbitAA=max(fwidth(radius),0.0005);
    float orbit=1-smoothstep(0.0007,0.0007+orbitAA,edge);
    float arc=smoothstep(-0.6,0.5,sin(atan2(fromCenter.y,fromCenter.x)*2+0.8));
    color=lerp(color,lerp(_CyanColor.rgb,_SecondaryGlow.rgb,step(0,fromCenter.x)),orbit*arc*_OrbitOpacity);

    // Source alpha is sampled directly; the logo is never color-keyed or recolored.
    if(_BrandOpacity>0)
    {
        float2 local=fromCenter/size+0.5;
        float inside=step(0,local.x)*step(local.x,1)*step(0,local.y)*step(local.y,1);
        float2 edgeDistance=min(local,1-local);
        inside*=smoothstep(0,max(_BrandFeather,0.001),min(edgeDistance.x,edgeDistance.y));
        fixed4 artwork=tex2D(_BrandTex,lerp(_BrandRect.xy,_BrandRect.zw,saturate(local)));
        color=lerp(color,artwork.rgb,artwork.a*inside*_BrandOpacity*smoothstep(0.55,0.96,_Progress));
    }
    return color;
}
