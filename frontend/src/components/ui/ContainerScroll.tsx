import { useEffect, useRef, useState } from 'react'
import { motion, type MotionValue, useScroll, useTransform } from 'framer-motion'

export function ContainerScroll({ titleComponent, children }: { titleComponent: React.ReactNode; children: React.ReactNode }) {
  const containerRef = useRef<HTMLDivElement>(null)
  const { scrollYProgress } = useScroll({ target: containerRef, offset: ['start end', 'end start'] })
  const [isMobile, setIsMobile] = useState(false)

  useEffect(() => {
    const checkMobile = () => setIsMobile(window.innerWidth <= 768)
    checkMobile()
    window.addEventListener('resize', checkMobile)
    return () => window.removeEventListener('resize', checkMobile)
  }, [])

  const scaleDimensions = isMobile ? [0.85, 0.95] : [1.05, 1]

  const rotate = useTransform(scrollYProgress, [0, 1], [20, 0])
  const scale = useTransform(scrollYProgress, [0, 1], scaleDimensions)
  const translate = useTransform(scrollYProgress, [0, 1], [0, -60])

  return (
    <div ref={containerRef} className="relative flex h-[42rem] items-center justify-center p-2 sm:h-[50rem] md:p-8">
      <div className="relative w-full py-6 md:py-14" style={{ perspective: '1000px' }}>
        <Header translate={translate}>{titleComponent}</Header>
        <ScrollCard rotate={rotate} scale={scale}>
          {children}
        </ScrollCard>
      </div>
    </div>
  )
}

function Header({ translate, children }: { translate: MotionValue<number>; children: React.ReactNode }) {
  return (
    <motion.div style={{ translateY: translate }} className="mx-auto max-w-2xl text-center">
      {children}
    </motion.div>
  )
}

function ScrollCard({
  rotate,
  scale,
  children,
}: {
  rotate: MotionValue<number>
  scale: MotionValue<number>
  children: React.ReactNode
}) {
  return (
    <motion.div
      style={{
        rotateX: rotate,
        scale,
        boxShadow: '0 0 #0000004d, 0 9px 20px #0000004a, 0 37px 37px #00000042, 0 84px 50px #00000026',
      }}
      className="mx-auto mt-10 h-[24rem] w-full max-w-4xl rounded-[24px] border border-border bg-foreground p-2 shadow-2xl sm:h-[30rem] md:p-4"
    >
      <div className="h-full w-full overflow-hidden rounded-[16px] bg-card p-3 sm:p-6">{children}</div>
    </motion.div>
  )
}
