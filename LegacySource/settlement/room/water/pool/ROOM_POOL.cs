using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Room.Water.Pool
{
    using Game.Time;
    using Init.Constant;
    using Init.Sprite;
    using Settlement.Main;
    using Settlement.Misc.Util;
    using Settlement.Path.Availability;
    using Settlement.Path.Finders;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Main.Util;
    using Settlement.Room.Sprite;
    using Settlement.Room.Water;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Graphic;
    using Snake2D.Util.Math;
    using Snake2D.Util.Render;

    public class RoomPool : RoomBlueprintImp, RoomPumpable
    {
        private static readonly long SerialVersionUID = 1L;

        public static readonly string KEY = "room.pool";

        public static readonly int[] UPGRATES = new int[] { 0, 1, 2 };

        public static readonly double[] FERTILITY = new double[] { 0.5, 0.5, 0.5 };

        public static readonly int[] WORKERS = new int[] { 2, 4, 6 };

        private Instance instance;

        private Canal canal;

        private Canal[] canals = new Canal[DIR.ORTHO.Length];

        private Canal[] canalConns = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons2 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons3 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons4 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons5 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons6 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons7 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons8 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons9 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons10 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons11 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons12 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons13 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons14 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons15 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons16 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons17 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons18 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons19 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons20 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons21 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons22 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons23 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons24 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons25 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons26 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons27 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons28 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons29 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons30 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons31 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons32 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons33 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons34 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons35 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons36 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons37 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons38 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons39 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons40 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons41 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons42 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons43 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons44 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons45 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons46 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons47 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons48 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons49 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons50 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons51 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons52 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons53 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons54 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons55 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons56 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons57 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons58 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons59 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons60 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons61 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons62 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons63 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons64 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons65 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons66 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons67 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons68 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons69 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons70 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons71 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons72 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons73 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons74 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons75 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons76 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons77 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons78 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons79 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons80 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons81 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons82 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons83 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons84 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons85 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons86 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons87 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons88 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons89 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons90 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons91 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons92 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons93 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons94 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons95 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons96 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons97 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons98 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons99 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons100 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons101 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons102 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons103 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons104 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons105 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons106 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons107 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons108 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons109 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons110 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons111 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons112 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons113 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons114 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons115 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons116 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons117 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons118 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons119 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons120 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons121 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons122 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons123 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons124 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons125 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons126 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons127 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons128 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons129 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons130 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons131 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons132 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons133 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons134 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons135 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons136 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons137 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons138 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons139 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons140 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons141 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons142 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons143 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons144 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons145 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons146 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons147 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons148 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons149 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons150 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons151 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons152 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons153 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons154 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons155 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons156 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons157 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons158 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons159 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons160 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons161 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons162 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons163 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons164 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons165 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons166 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons167 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons168 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons169 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons170 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons171 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons172 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons173 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons174 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons175 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons176 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons177 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons178 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons179 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons180 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons181 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons182 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons183 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons184 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons185 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons186 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons187 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons188 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons189 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons190 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons191 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons192 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons193 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons194 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons195 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons196 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons197 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons198 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons199 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons200 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons201 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons202 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons203 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons204 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons205 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons206 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons207 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons208 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons209 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons210 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons211 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons212 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons213 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons214 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons215 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons216 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons217 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons218 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons219 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons220 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons221 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons222 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons223 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons224 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons225 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons226 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons227 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons228 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons229 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons230 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons231 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons232 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons233 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons234 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons235 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons236 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons237 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons238 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons239 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons240 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons241 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons242 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons243 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons244 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons245 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons246 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons247 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons248 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons249 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons250 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons251 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons252 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons253 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons254 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons255 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons256 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons257 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons258 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons259 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons260 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons261 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons262 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons263 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons264 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons265 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons266 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons267 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons268 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons269 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons270 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons271 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons272 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons273 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons274 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons275 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons276 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons277 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons278 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons279 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons280 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons281 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons282 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons283 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons284 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons285 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons286 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons287 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons288 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons289 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons290 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons291 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons292 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons293 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons294 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons295 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons296 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons297 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons298 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons299 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons300 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons301 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons302 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons303 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons304 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons305 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons306 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons307 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons308 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons309 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons310 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons311 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons312 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons313 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons314 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons315 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons316 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons317 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons318 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons319 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons320 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons321 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons322 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons323 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons324 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons325 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons326 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons327 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons328 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons329 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons330 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons331 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons332 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons333 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons334 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons335 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons336 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons337 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons338 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons339 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons340 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons341 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons342 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons343 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons344 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons345 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons346 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons347 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons348 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons349 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons350 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons351 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons352 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons353 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons354 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons355 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons356 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons357 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons358 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons359 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons360 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons361 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons362 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons363 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons364 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons365 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons366 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons367 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons368 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons369 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons370 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons371 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons372 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons373 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons374 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons375 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons376 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons377 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons378 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons379 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons380 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons381 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons382 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons383 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons384 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons385 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons386 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons387 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons388 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons389 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons390 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons391 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons392 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons393 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons394 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons395 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons396 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons397 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons398 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons399 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons400 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons401 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons402 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons403 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons404 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons405 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons406 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons407 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons408 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons409 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons410 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons411 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons412 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons413 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons414 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons415 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons416 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons417 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons418 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons419 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons420 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons421 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons422 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons423 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons424 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons425 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons426 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons427 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons428 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons429 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons430 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons431 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons432 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons433 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons434 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons435 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons436 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons437 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons438 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons439 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons440 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons441 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons442 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons443 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons444 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons445 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons446 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons447 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons448 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons449 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons450 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons451 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons452 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons453 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons454 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons455 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons456 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons457 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons458 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons459 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons460 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons461 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons462 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons463 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons464 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons465 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons466 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons467 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons468 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons469 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons470 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons471 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons472 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons473 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons474 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons475 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons476 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons477 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons478 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons479 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons480 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons481 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons482 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons483 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons484 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons485 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons486 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons487 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons488 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons489 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons490 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons491 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons492 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons493 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons494 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons495 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons496 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons497 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons498 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons499 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons500 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons501 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons502 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons503 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons504 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons505 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons506 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons507 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons508 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons509 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons510 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons511 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons512 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons513 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons514 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons515 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons516 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons517 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons518 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons519 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons520 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons521 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons522 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons523 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons524 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons525 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons526 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons527 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons528 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons529 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons530 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons531 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons532 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons533 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons534 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons535 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons536 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons537 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons538 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons539 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons540 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons541 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons542 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons543 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons544 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons545 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons546 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons547 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons548 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons549 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons550 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons551 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons552 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons553 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons554 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons555 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons556 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons557 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons558 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons559 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons560 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons561 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons562 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons563 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons564 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons565 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons566 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons567 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons568 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons569 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons570 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons571 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons572 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons573 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons574 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons575 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons576 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons577 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons578 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons579 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons580 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons581 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons582 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons583 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons584 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons585 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons586 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons587 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons588 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons589 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons590 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons591 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons592 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons593 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons594 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons595 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons596 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons597 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons598 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons599 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons600 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons601 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons602 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons603 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons604 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons605 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons606 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons607 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons608 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons609 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons610 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons611 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons612 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons613 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons614 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons615 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons616 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons617 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons618 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons619 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons620 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons621 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons622 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons623 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons624 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons625 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons626 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons627 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons628 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons629 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons630 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons631 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons632 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons633 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons634 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons635 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons636 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons637 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons638 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons639 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons640 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons641 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons642 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons643 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons644 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons645 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons646 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons647 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons648 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons649 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons650 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons651 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons652 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons653 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons654 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons655 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons656 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons657 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons658 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons659 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons660 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons661 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons662 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons663 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons664 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons665 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons666 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons667 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons668 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons669 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons670 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons671 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons672 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons673 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons674 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons675 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons676 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons677 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons678 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons679 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons680 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons681 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons682 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons683 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons684 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons685 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons686 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons687 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons688 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons689 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons690 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons691 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons692 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons693 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons694 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons695 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons696 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons697 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons698 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons699 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons700 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons701 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons702 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons703 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons704 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons705 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons706 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons707 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons708 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons709 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons710 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons711 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons712 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons713 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons714 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons715 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons716 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons717 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons718 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons719 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons720 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons721 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons722 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons723 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons724 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons725 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons726 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons727 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons728 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons729 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons730 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons731 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons732 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons733 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons734 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons735 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons736 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons737 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons738 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons739 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons740 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons741 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons742 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons743 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons744 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons745 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons746 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons747 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons748 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons749 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons750 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons751 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons752 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons753 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons754 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons755 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons756 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons757 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons758 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons759 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons760 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons761 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons762 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons763 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons764 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons765 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons766 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons767 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons768 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons769 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons770 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons771 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons772 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons773 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons774 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons775 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons776 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons777 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons778 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons779 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons780 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons781 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons782 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons783 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons784 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons785 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons786 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons787 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons788 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons789 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons790 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons791 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons792 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons793 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons794 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons795 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons796 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons797 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons798 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons799 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons800 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons801 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons802 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons803 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons804 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons805 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons806 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons807 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons808 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons809 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons810 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons811 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons812 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons813 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons814 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons815 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons816 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons817 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons818 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons819 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons820 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons821 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons822 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons823 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons824 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons825 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons826 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons827 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons828 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons829 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons830 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons831 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons832 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons833 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons834 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons835 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons836 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons837 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons838 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons839 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons840 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons841 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons842 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons843 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons844 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons845 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons846 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons847 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons848 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons849 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons850 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons851 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons852 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons853 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons854 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons855 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons856 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons857 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons858 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons859 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons860 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons861 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons862 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons863 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons864 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons865 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons866 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons867 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons868 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons869 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons870 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons871 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons872 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons873 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons874 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons875 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons876 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons877 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons878 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons879 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons880 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons881 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons882 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons883 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons884 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons885 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons886 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons887 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons888 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons889 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons890 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons891 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons892 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons893 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons894 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons895 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons896 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons897 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons898 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons899 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons900 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons901 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons902 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons903 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons904 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons905 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons906 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons907 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons908 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons909 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons910 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons911 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons912 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons913 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons914 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons915 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons916 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons917 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons918 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons919 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons920 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons921 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons922 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons923 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons924 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons925 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons926 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons927 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons928 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons929 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons930 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons931 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons932 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons933 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons934 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons935 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons936 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons937 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons938 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons939 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons940 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons941 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons942 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons943 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons944 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons945 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons946 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons947 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons948 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons949 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons950 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons951 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons952 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons953 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons954 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons955 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons956 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons957 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons958 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons959 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons960 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons961 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons962 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons963 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons964 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons965 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons966 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons967 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons968 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons969 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons970 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons971 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons972 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons973 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons974 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons975 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons976 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons977 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons978 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons979 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons980 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons981 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons982 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons983 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons984 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons985 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons986 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons987 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons988 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons989 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons990 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons991 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons992 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons993 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons994 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons995 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons996 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons997 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons998 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons999 = new Canal[DIR.ORTHO.Length];

        private Canal[] canalCons1000 = new Canal[DIR.ORTHO.Length];