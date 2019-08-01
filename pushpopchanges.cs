public static void Emulator(int initPC, int codeLen, int initSP,
                                InFile data, OutFile results, bool tracing,
                                bool traceStack, bool traceHeap) {
    // Emulates action of the codeLen instructions stored in mem[0 .. codeLen-1], with
    // program counter initialized to initPC, stack pointer initialized to initSP.
    // data and results are used for I/O.  Tracing at the code level may be requested

      int pcNow;                  // current program counter
      int loop;                   // internal loops
      int tos, sos;               // value popped from stack
      int adr;                    // effective address for memory accesses
      stackBase = initSP;
      heapBase = codeLen;         // initialize boundaries
      cpu.hp = heapBase;          // initialize registers
      cpu.sp = stackBase;
      cpu.gp = stackBase;
      cpu.mp = stackBase;
      cpu.fp = stackBase;
      cpu.pc = initPC;            // initialize program counter
      ps = running;               // prepare to execute
      int ops = 0;
      timer.Start();
      do {
        ops++;
        pcNow = cpu.pc;           // retain for tracing/postmortem
        if (cpu.pc < 0 || cpu.pc >= codeLen) {
          ps = badAdr;
          break;
        }
        cpu.ir = Next();          // fetch
        if (tracing) Trace(results, pcNow, traceStack, traceHeap);
        switch (cpu.ir) {         // execute
          case PVM.nop:           // no operation
            break;
          case PVM.dsp:           // decrement stack pointer (allocate space for variables)
            int localSpace = Next();
            cpu.sp -= localSpace;
            if (InBounds(cpu.sp)) // initialize
              for (loop = 0; loop < localSpace; loop++)
                mem[cpu.sp + loop] = 0;
            break;
          case PVM.ldc:           // push constant value
            Push(Next());
            break;
          case PVM.lda:           // push local address
            adr = cpu.fp - 1 - Next();
            if (InBounds(adr)) Push(adr);
            break;
          case PVM.ldv:           // dereference
            Push(mem[Pop()]);
            break;
          case PVM.sto:           // store
            tos = Pop(); adr = Pop();
            if (InBounds(adr)) mem[adr] = tos;
            break;
          case PVM.ldxa:          // heap array indexing
            adr = Pop();
            int heapPtr = Pop();
            if (heapPtr == 0) ps = nullRef;
            else if (heapPtr < heapBase || heapPtr >= cpu.hp) ps = badMem;
            else if (adr < 0 || adr >= mem[heapPtr]) ps = badInd;
            else Push(heapPtr + adr + 1);
            break;
          case PVM.inpi:          // integer input
            adr = Pop();
            if (InBounds(adr)) {
              mem[adr] = data.ReadInt();
              if (data.Error()) ps = badData;
            }
            break;
          case PVM.prni:          // integer output
            if (tracing) results.Write(padding);
            results.Write(Pop(), 0);
            if (tracing) results.WriteLine();
            break;
          case PVM.inpb:          // boolean input
            adr = Pop();
            if (InBounds(adr)) {
              mem[adr] = data.ReadBool() ? 1 : 0;
              if (data.Error()) ps = badData;
            }
            break;
          case PVM.prnb:          // boolean output
            if (tracing) results.Write(padding);
            if (Pop() != 0) results.Write(" true  "); else results.Write(" false ");
            if (tracing) results.WriteLine();
            break;
          case PVM.prns:          // string output
            if (tracing) results.Write(padding);
            loop = Next();
            while (ps == running && mem[loop] != 0) {
              results.Write((char) mem[loop]); loop--;
              if (loop < stackBase) ps = badMem;
            }
            if (tracing) results.WriteLine();
            break;
          case PVM.prnl:          // newline
            results.WriteLine();
            break;
          case PVM.neg:           // integer negation
            Push(-Pop());
            break;
          case PVM.add:           // integer addition
            tos = Pop(); Push(Pop() + tos);
            break;
          case PVM.sub:           // integer subtraction
            tos = Pop(); Push(Pop() - tos);
            break;
          case PVM.mul:           // integer multiplication
            tos = Pop(); 
            sos = Pop();
            int freeSpace = (memSize - cpu.hp - (memSize - cpu.sp));
            if((freeSpace / tos) > sos)
            {
              Push(sos * tos);
            }
            else
            {
              ps = badVal;
            }
            
            break;
          case PVM.div:           // integer division (quotient)
            tos = Pop();
            if (tos == 0)
            {
              ps = divZero;
            }
            else
            {
              Push(Pop() / tos);    
            }
            break;
          case PVM.rem:           // integer division (remainder)
            tos = Pop(); Push(Pop() % tos);
            break;
          case PVM.not:           // logical negation
            Push(Pop() == 0 ? 1 : 0);
            break;
          case PVM.and:           // logical and
            tos = Pop(); Push(Pop() & tos);
            break;
          case PVM.or:            // logical or
            tos = Pop(); Push(Pop() | tos);
            break;
          case PVM.ceq:           // logical equality
            tos = Pop(); Push(Pop() == tos ? 1 : 0);
            break;
          case PVM.cne:           // logical inequality
            tos = Pop(); Push(Pop() != tos ? 1 : 0);
            break;
          case PVM.clt:           // logical less
            tos = Pop(); Push(Pop() <  tos ? 1 : 0);
            break;
          case PVM.cle:           // logical less or equal
            tos = Pop(); Push(Pop() <= tos ? 1 : 0);
            break;
          case PVM.cgt:           // logical greater
            tos = Pop(); Push(Pop() >  tos ? 1 : 0);
            break;
          case PVM.cge:           // logical greater or equal
            tos = Pop(); Push(Pop() >= tos ? 1 : 0);
            break;
          case PVM.brn:           // unconditional branch
            cpu.pc = Next();
            if (cpu.pc < 0 || cpu.pc >= codeLen) ps = badAdr;
            break;
          case PVM.bze:           // pop top of stack, branch if false
            int target = Next();
            if (Pop() == 0) {
              cpu.pc = target;
              if (cpu.pc < 0 || cpu.pc >= codeLen) ps = badAdr;
            }
            break;
          case PVM.anew:          // heap array allocation
            int size = Pop();
            if (size <= 0 || size + 1 > cpu.sp - cpu.hp - 2)
              ps = badAll;
            else {
              mem[cpu.hp] = size;
              Push(cpu.hp);
              cpu.hp += size + 1;
            }
            break;
          case PVM.halt:          // halt
            ps = finished;
            break;
          case PVM.stk:           // stack dump (debugging)
            StackDump(results, pcNow);
            break;
          case PVM.heap:           // heap dump (debugging)
            HeapDump(results, pcNow);
            break;
          case PVM.ldc_0:         // push constant 0
			Push(0);
			break;
          case PVM.ldc_1:         // push constant 1
			Push(1);
			break;
          case PVM.ldc_2:         // push constant 2
			Push(2);
			break;
          case PVM.ldc_3:         // push constant 3
			Push(3);
			break;
          case PVM.lda_0:         // push local address 0
			adr = cpu.fp - 1 - 0;
            if (InBounds(adr)) Push(adr);
            break;
          case PVM.lda_1:         // push local address 1
			adr = cpu.fp - 1 - 1;
            if (InBounds(adr)) Push(adr);
            break;		  
          case PVM.lda_2:         // push local address 2
		  	adr = cpu.fp - 1 - 2;
            if (InBounds(adr)) Push(adr);
            break;
          case PVM.lda_3:         // push local address 3
		  	adr = cpu.fp - 1 - 3;
            if (InBounds(adr)) Push(adr);
            break;
          case PVM.ldl:           // push local value
		    adr = cpu.fp - 1 - Next();
            if (InBounds(adr)) Push(adr);
			Push(mem[Pop()]);
			break;
          case PVM.ldl_0:         // push value of local variable 0
			adr = cpu.fp - 1 - 0;
            if (InBounds(adr)) Push(adr);
			Push(mem[Pop()]);
			break;
          case PVM.ldl_1:         // push value of local variable 1
		  	adr = cpu.fp - 1 - 1;
            if (InBounds(adr)) Push(adr);
			Push(mem[Pop()]);
			break;
          case PVM.ldl_2:         // push value of local variable 2
		  	adr = cpu.fp - 1 - 2;
            if (InBounds(adr)) Push(adr);
			Push(mem[Pop()]);
			break;
          case PVM.ldl_3:         // push value of local variable 3
		  	adr = cpu.fp - 1 - 3;
            if (InBounds(adr)) Push(adr);
			Push(mem[Pop()]);
			break;
          case PVM.stl:           // store local value
		    adr = cpu.fp - 1 - Next();
            if (InBounds(adr)) Push(adr);
			tos = Pop();
            if (InBounds(adr)) mem[adr] = Pop();
			break;
          case PVM.stlc:          // store local value
          case PVM.stl_0:         // pop to local variable 0
		    adr = cpu.fp - 1 - 0;
            if (InBounds(adr)) Push(adr);
			tos = Pop();
            if (InBounds(adr)) mem[adr] = Pop();
			break;
          case PVM.stl_1:         // pop to local variable 1
		    adr = cpu.fp - 1 - 1;
            if (InBounds(adr)) Push(adr);
			tos = Pop();
            if (InBounds(adr)) mem[adr] = Pop();
			break;
          case PVM.stl_2:         // pop to local variable 2
		    adr = cpu.fp - 1 - 2;
            if (InBounds(adr)) Push(adr);
			tos = Pop();
            if (InBounds(adr)) mem[adr] = Pop();
			break;
          case PVM.stl_3:         // pop to local variable 3
		    adr = cpu.fp - 1 - 3;
            if (InBounds(adr)) Push(adr);
			tos = Pop();
            if (InBounds(adr)) mem[adr] = Pop();
			break;
          case PVM.stoc:
            tos = Pop();
            if (64 < tos && tos < 91 || 96 < tos && tos < 123)
            {
                tos = Pop(); adr = Pop();
                if (InBounds(adr)) mem[adr] = tos;
            }
            ps = badOp;
      break;// character checked store
          case PVM.inpc:          // char input
            Push(data.ReadChar());
      break;
          case PVM.prnc:          // integer output
            results.Write((char)Pop(), 0);
      break;
          //case PVM.inpc:          // character input         // character output
          case PVM.cap:
          tos = Pop();
            if (96 < tos && tos < 123)
            {
              Push(tos -32);
            }
            else
            {
                ps = badOp;
            }
      break;           // toUpperCase
          case PVM.low: 
          tos = Pop();
            if (64 < tos && tos < 91)
            {
              Push(tos + 31);
            }
            else
            {
                ps = badOp;
            }
      break;          // toLowerCase
          case PVM.islet:  
          tos = Pop();
          if (64 < tos && tos < 91 || 96 < tos && tos < 123)
          {
              Push(1);
          }else
          {
              Push(0);   
          }       // isLetter
          case PVM.inc: 
          tos = Pop();           // ++  //NOT DONE PROPERLY 
          Push(tos += 1);
      break;// ++
          case PVM.dec: 
          tos = Pop();           // ++  //NOT DONE PROPERLY 
          Push(tos -= 1);
      break;          // --
          default:                // unrecognized opcode
            ps = badOp;
            break;
        }
      } while (ps == running);
      TimeSpan ts = timer.Elapsed;

      // Format and display the TimeSpan value.
      string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                         ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
      Console.WriteLine("\n\n" + ops + " operations.  Run Time " + elapsedTime + "\n\n");
      if (ps != finished) PostMortem(results, pcNow);
      timer.Reset();
      timer.Stop();
    } // PVM.Emulator
