let makeStr32hexAddress = (xx______=0, __xx____=0, ____xx__=0, ______xx=0)=>{
    return  (xx______>0xF?'':'0') + xx______.toString(16).toUpperCase() +
            (__xx____>0xF?'':'0') + __xx____.toString(16).toUpperCase() +
            (____xx__>0xF?'':'0') + ____xx__.toString(16).toUpperCase() +
            (______xx>0xF?'':'0') + ______xx.toString(16).toUpperCase() ;
};

// JnBBgInt : JAVA nio.ByteBuffer.getInt
let JnBBgInt = (array)=>{
    return parseInt(makeStr32hexAddress(...array), 16);
};



let getGeometryData = (bytes)=>{

	// source :
	// https://github.com/GloriousLiar/DK64FileParsers/blob/main/src/Main.java

    let consoleTxt = '';

    consoleTxt += "Begin.\n";

    let dlStart          = [bytes[52],bytes[53],bytes[54],bytes[55]];
    let vertStart        = [bytes[56],bytes[57],bytes[58],bytes[59]];
    let vertEnd          = [bytes[64],bytes[65],bytes[66],bytes[67]];
    let sectionStart     = [bytes[88],bytes[89],bytes[90],bytes[91]];
    let sectionEnd       = [bytes[92],bytes[93],bytes[94],bytes[95]];
    let chunkCountOffset = [bytes[100],bytes[101],bytes[102],bytes[103]];
    let chunkStart       = [bytes[104],bytes[105],bytes[106],bytes[107]];

	let structHexStr = {
		DL_Start :           makeStr32hexAddress(dlStart[0],dlStart[1],dlStart[2],dlStart[3]),
		Vert_Start :         makeStr32hexAddress(vertStart[0],vertStart[1],vertStart[2],vertStart[3]),
		Vert_End :           makeStr32hexAddress(vertEnd[0],vertEnd[1],vertEnd[2],vertEnd[3]),
		Section_Start :      makeStr32hexAddress(sectionStart[0],sectionStart[1],sectionStart[2],sectionStart[3]),
		Section_End :        makeStr32hexAddress(sectionEnd[0],sectionEnd[1],sectionEnd[2],sectionEnd[3]),
		Chunk_Count_Offset : makeStr32hexAddress(chunkCountOffset[0],chunkCountOffset[1],chunkCountOffset[2],chunkCountOffset[3]),
		Chunk_Start :        makeStr32hexAddress(chunkStart[0],chunkStart[1],chunkStart[2],chunkStart[3]),
	};

	let structOutput = {
		DL_Start :           parseInt(structHexStr.DL_Start, 16),
		Vert_Start :         parseInt(structHexStr.Vert_Start, 16),
		Vert_End :           parseInt(structHexStr.Vert_End, 16),
		Section_Start :      parseInt(structHexStr.Section_Start, 16),
		Section_End :        parseInt(structHexStr.Section_End, 16),
		Chunk_Count_Offset : parseInt(structHexStr.Chunk_Count_Offset, 16),
		Chunk_Start :        parseInt(structHexStr.Chunk_Start, 16),
		RawChunkData :       null,
		ChunkData :          null,
		ChunkCount :         null,
	};

    let sOut = "**** Header Data ****\n";
    sOut += "DL Start: "          + structHexStr.DL_Start + "\n" +
            "Vert Start: "        + structHexStr.Vert_Start + "\n" +
            "Vert End: "          + structHexStr.Vert_End + "\n" +
            "Section Start: "     + structHexStr.Section_Start + "\n" +
            "Section End: "       + structHexStr.Section_End + "\n" +
            "Chunk Count Offset: "+ structHexStr.Chunk_Count_Offset + "\n" + 
            "Chunk Start: "       + structHexStr.Chunk_Start + "\n" +
            "*********************\n";
    
    sOut += "**** Chunk Data ****\n";
    let chunkOffset = JnBBgInt(chunkCountOffset);

    // byte[]
    let numCh = [bytes[chunkOffset],bytes[chunkOffset+1],bytes[chunkOffset+2],bytes[chunkOffset+3]];

    let numChunks = JnBBgInt(numCh);
	structOutput.ChunkCount = numChunks;
    sOut += "Number of Chunks: "+numChunks + "\n";
    
    // ArrayList<byte[]> chunkData = new ArrayList<byte[]>();
    let chunkData = [];
	let chunkFormat = [];

    let chunkSize = 52; //chunk size = 0x34 = 52 bytes
    let chunkStartOffset = JnBBgInt(chunkStart);
    for(let i=0; i<numChunks; ++i) {

        // byte[] chunkBytes = new byte[chunkSize];
        let chunkBytes = [];
		let chunkStruct = {x:0,y:0, DL:[{},{},{},{}], vert:{}};

        for(let j=0; j<52; ++j) {
            chunkBytes[j] = bytes[chunkStartOffset + i*chunkSize + j];
        }
        chunkData.push(chunkBytes);
		chunkFormat.push(chunkStruct);

        let x = [chunkBytes[0],chunkBytes[1],chunkBytes[2],chunkBytes[3]];
        let y = [chunkBytes[4],chunkBytes[5],chunkBytes[6],chunkBytes[7]];
		chunkStruct.x = JnBBgInt(x);
		chunkStruct.y = JnBBgInt(y);
        sOut += "** Chunk "+(i+1)+": **\n" + 
                "x: "+chunkStruct.x + "\n" +
                "y: "+chunkStruct.y + "\n";

        sOut += "** DL table: \n";
        for(let j=0; j<4; ++j) {
            let dlOffsets = [chunkBytes[12 + j*8 + 0],chunkBytes[12 + j*8 + 1],chunkBytes[12 + j*8 + 2],chunkBytes[12 + j*8 + 3]];
            let dlSizes   = [chunkBytes[12 + j*8 + 4],chunkBytes[12 + j*8 + 5],chunkBytes[12 + j*8 + 6],chunkBytes[12 + j*8 + 7]];
			let dlOffsetsHexStr = makeStr32hexAddress(dlOffsets[0],dlOffsets[1],dlOffsets[2],dlOffsets[3]);
			let dlSizesHexStr   = makeStr32hexAddress(dlSizes[0],dlSizes[1],dlSizes[2],dlSizes[3]);
			chunkStruct.DL[j].o = parseInt(dlOffsetsHexStr, 16);
			chunkStruct.DL[j].s = parseInt(dlSizesHexStr, 16);
            sOut += "**** DL " + (j+1) + " Offset: " + dlOffsetsHexStr + "\n" +
                    "**** DL " + (j+1) + " Sizes: "  + dlSizesHexStr   + "\n" + 
                    "****\n";
        }
        let vertOffset = [chunkBytes[44],chunkBytes[45],chunkBytes[46],chunkBytes[47]];
        let vertSize   = [chunkBytes[48],chunkBytes[49],chunkBytes[50],chunkBytes[51]];
		let vertOffsetHexStr = makeStr32hexAddress(vertOffset[0],vertOffset[1],vertOffset[2],vertOffset[3]);
		let vertSizeHexStr   = makeStr32hexAddress(vertSize[0],vertSize[1],vertSize[2],vertSize[3]);
		chunkStruct.vert.o = parseInt(vertOffsetHexStr, 16);
		chunkStruct.vert.s = parseInt(vertSizeHexStr, 16);
        sOut += "Vert Offset: " + vertOffsetHexStr + "\n" +
                "Vert Size: "   + vertSizeHexStr + "\n" +
                "****\n";
    }
    
	// section getting
	//

	let sectionInfoBytes = bytes.slice(structOutput.Section_Start, structOutput.Section_End);
	let sectionInfoCount = JnBBgInt(sectionInfoBytes.splice(0, 4));
	//console.log(sectionInfoBytes.map(e=>(e<=0xF?'0':'')+e.toString(16).toUpperCase()));
	let sectionInfoData = [];
	while(sectionInfoBytes.length > 27) sectionInfoData.push(sectionInfoBytes.splice(0,28));
	let sectionInfoStruct = [];

	// DK64-Viewer ref inspiration
	sectionInfoData.forEach(e=>sectionInfoStruct.push({
		sectionID : JnBBgInt([0,0, e[0 ], e[1]]),
		meshID    : JnBBgInt([0,0, e[2 ], e[3]]),
		vOfst1    : JnBBgInt([0,0, e[8 ], e[9]]),
		vOfst_    : JnBBgInt([0,0, e[10], e[11]]),
		vOfst2    : JnBBgInt([0,0, e[12], e[13]]),
		vOfst3    : JnBBgInt([0,0, e[14], e[15]]),
		vSize1    : JnBBgInt([0,0, e[16], e[17]]),
		vSize_    : JnBBgInt([0,0, e[18], e[19]]),
		vSize2    : JnBBgInt([0,0, e[20], e[21]]),
		vSize3    : JnBBgInt([0,0, e[22], e[23]]),
	}));

	// rebuild (with new name & )
	//
	// ordered by meshID
	sectionInfoStruct = sectionInfoStruct.sort((A,B)=>{
		return (A.meshID < B.meshID) ? -1 : +1;
	// rename props
	}).map(e=>({
		section  : e.sectionID,
		mesh     : e.meshID,
		DL1vOfst : e.vOfst1, // vertex index offset (not byte address offset)
		DL2vOfst : e.vOfst_, // vertex index offset (not byte address offset)
		DL3vOfst : e.vOfst2, // vertex index offset (not byte address offset)
		DL4vOfst : e.vOfst3, // vertex index offset (not byte address offset)
		DL1vSize : e.vSize1,
		DL2vSize : e.vSize_,
		DL3vSize : e.vSize2,
		DL4vSize : e.vSize3,
	}));
	// add default empty (meshID 0)
	sectionInfoStruct.unshift({
		section:0,mesh:0,
		DL1vOfst:0,DL2vOfst:0,DL3vOfst:0,DL4vOfst:0,
		DL1vSize:0,DL2vSize:0,DL3vSize:0,DL4vSize:0
	});

	structOutput.Sections = sectionInfoStruct;

	console.table(sectionInfoStruct)

    sOut += "*********************\n";
    
    consoleTxt += sOut;
    consoleTxt += "End."

//    console.log(consoleTxt);

	// fix patch (for file with no chunk)
	let fileVertCount = structOutput.Vert_End - structOutput.Vert_Start;
	if(numChunks === 0){
		chunkFormat.push({x:0,y:0, DL:[{},{},{},{}], vert:{o:0,s:fileVertCount}});
		structOutput.ChunkCount = 1;
	}
		
		
		
	structOutput.RawChunkData = chunkData;
	structOutput.ChunkData = chunkFormat;

	structOutput.log = consoleTxt;

    return structOutput;
}