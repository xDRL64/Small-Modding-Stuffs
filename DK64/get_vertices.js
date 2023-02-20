let getFloat32_from2Bytes = (A,B)=>{
	//let UI8A = new Uint8Array([0,0,A,B]);
	//let UI8A = new Uint8Array([B,A,0,0]);
	//let UI8A = new Uint8Array([A,B,0,0]);
	//return (new Float32Array(UI8A.buffer))[0];

//	//let UI8A = new Uint8Array([A,B]);
//	let UI8A = new Uint8Array([B,A]);
//	return (new Float16Array(UI8A.buffer))[0];

//	//let UI8A = new Uint8Array([B,A]);
//	let UI8A = new Uint8Array([A,B]);
//	return (new Int16Array(UI8A.buffer))[0];

	return (A<<8)+B;
//	return (B<<8)+A;
};

let get_vertices = (binFile, vertStartAddress, vertOffset, vertByteSize)=>{

	let address = vertStartAddress+vertOffset;

	let data = binFile.slice(address, address+vertByteSize);

	let vertCount = vertByteSize >> 4; // div by 16

	let vertices = [];

	for(let iVert=0; iVert<vertCount; iVert++){
		let iVertOfst = iVert << 4; // mul by 16
		let vertex = {x:0,y:0,z:0, u:0,v:0, r:0,g:0,b:0,a:0,};

		vertex.x = getFloat32_from2Bytes(data[iVertOfst  ], data[iVertOfst+1]);
		vertex.y = getFloat32_from2Bytes(data[iVertOfst+2], data[iVertOfst+3]);
		vertex.z = getFloat32_from2Bytes(data[iVertOfst+4], data[iVertOfst+5]);

		vertex.u = getFloat32_from2Bytes(data[iVertOfst+8 ], data[iVertOfst+9 ]);
		vertex.v = getFloat32_from2Bytes(data[iVertOfst+10], data[iVertOfst+11]);

		vertex.r = data[iVertOfst+12] / 255;
		vertex.g = data[iVertOfst+13] / 255;
		vertex.b = data[iVertOfst+14] / 255;
		vertex.a = data[iVertOfst+15] / 255;

		vertices.push(vertex);
	}

	return vertices;
};

let getDirectGeo_fromVerticesObj = (vObj)=>{
	return {
		position : vObj.map(e=>[e.x,e.y,e.z]).flat(),
		texcoord : vObj.map(e=>[e.u,e.v]).flat(),
		color    : vObj.map(e=>[e.r,e.g,e.b,e.a]).flat(),
	};
};

let compute_medianPoint = (verticesObj)=>{
	let xyzSum = verticesObj.reduce((acc,cur)=>[acc[0]+cur.x,acc[1]+cur.y,acc[2]+cur.z],[0,0,0]);
	return xyzSum.map(e=>e/verticesObj.length);
};

let get_2pointsLength = (pA, pB)=>{
	let A = vec3.create(); vec3.set(A, ...pA);
	let B = vec3.create(); vec3.set(B, ...pB);
	let C = vec3.create(); vec3.sub(C, A,B);
	return vec3.length(C);
};

let get_farestPointInfo = (verticesObj, center)=>{
	return verticesObj.reduce((acc,cur,i)=>{
		let dist = get_2pointsLength([cur.x,cur.y,cur.z],center);
		let longest = Math.max(acc.dist, dist);
		return {
			dist : longest,
			index: (acc.dist!==longest ? i : acc.index),
		};
	},{dist:-1,index:null});

}

let getFaces_fromDisplayList = (binFile, DLoffset, DLarray, SectionsByMesh, disabledDL)=>{
	let foundFaces = [];

	// debug (use checkboxes to display DL or not)
	DLarray = DLarray.map((e,i)=>(disabledDL[i]?e:{o:0xFFFFFFFF,s:0}));

	// debug
	let vertIndexCollection = new Set();


	let vBuffer = [0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0];
	let useSection = 0;

	DLarray.forEach((e,i)=>{
		if(e.s && (e.o!==0xFFFFFFFF)){
			let address = DLoffset + e.o;
			let end = address + e.s;
			let bytes = binFile.slice(address, end);
			let DLlist = [];
			while(bytes.length > 7) DLlist.push(bytes.splice(0,8));

			DLlist.every(e=>{

				// [CMD 00 : G_SPNOOP]
				if(e[0] === 0x00){
					console.log(
						`found : 0x00 in DL ${i+1} : ` + e.map(e=>(e<=0xF?'0':'')+e.toString(16).toUpperCase())
					);
					useSection = e[7];
				}

				// [CMD 01 : G_VTX]
				if(e[0] === 0x01){
					// https://hack64.net/wiki/doku.php?id=f3dex2#g_vtx
					// 01 0[N N]0 [II] [SS SS SS SS]
					let N = (((e[1]<<8)+e[2])&0x0FF0)>>4; // Number of vertices to write
					let I = e[3]-(N*2); // Where to start writing vertices inside the vertex buffer (start = II - N*2)

					//I = I >> 1; // like DK64-Viewer
					//I = (e[3]>>1)-(N); // exactly like DK64-Viewer

					let S = (e[5]<<16)+(e[6]<<8)+e[7]; // Segmented address to load vertices from

					//S+=(e[4]<<24);
					//S+=(0x01<<24);

					let iVert = S >> 4; // (div by 16) get vertices start index

					let sOffset = SectionsByMesh[useSection][`DL${i+1}vOfst`];

					for(let i=0; i<N; i++){	
						vBuffer[I+i] = iVert + i + sOffset;
					}

					// debug
					console.log(
						`found : 0x01 in DL ${i+1} : ` + e.map(e=>(e<=0xF?'0':'')+e.toString(16).toUpperCase())
					);
				}
				if(e[0] === 0x02){
					console.log(`found : 0x02 in DL ${i+1}`);
				}
				// [CMD 05] and [CMD 06 (first 4 bytes)]
				if(e[0] === 0x05 || e[0] === 0x06 || e[0] === 0x07){
					// https://hack64.net/wiki/doku.php?id=f3dex2#g_tri1
					// https://hack64.net/wiki/doku.php?id=f3dex2#g_tri2
					// 05 [AA] [BB] [CC] 00 00 00 00
					// 06 [AA] [BB] [CC] 00 -- -- --
					foundFaces.push([vBuffer[e[1]>>1],vBuffer[e[2]>>1],vBuffer[e[3]>>1]]);
				}
				// [CMD 06 (last 4 bytes)]
				if(e[0] === 0x06 || e[0] === 0x07){
					// https://hack64.net/wiki/doku.php?id=f3dex2#g_tri2
					// 06 -- -- -- 00 [DD] [EE] [FF]
					foundFaces.push([vBuffer[e[5]>>1],vBuffer[e[6]>>1],vBuffer[e[7]>>1]]);
				}

				if(e[0] === 0xDA){
					console.log(`found : 0xDA in DL ${i+1}`);
				}

				if(e[0] === 0xDE){
					console.log(`found : 0xDE in DL ${i+1}`);
				}
				if(e[0] === 0xDF){
					console.log(`found : 0xDF in DL ${i+1}`);
					useSection = 0;
					//return false;
				}

				return true;
			});
		}
	});

	return foundFaces;
};


let rebuildVertices_fromIndexedRef = (vertices, facesInfo, vertIndexOfst=0)=>{
	let output = [];
	facesInfo.forEach(e=>{
		output.push( ...(e.map(v=>vertices[v+vertIndexOfst])) );
	});
	return output;
};